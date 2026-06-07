using System.Text;
using System.Xml;
using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Services;

public class SpeechService
{
    private readonly HttpClient _httpClient;
    private readonly string _key;
    private readonly string _region;
    private readonly string _maleVoiceName;
    private readonly string _femaleVoiceName;

    public SpeechService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _key = configuration["AzureSpeech:Key"] ?? string.Empty;
        _region = configuration["AzureSpeech:Region"] ?? string.Empty;
        _maleVoiceName = configuration["AzureSpeech:MaleVoiceName"] ?? "es-CR-JuanNeural";
        _femaleVoiceName = configuration["AzureSpeech:FemaleVoiceName"] ?? "es-CR-MariaNeural";
    }

    public async Task<byte[]> SynthesizeAsync(
        SpeechRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_key) || string.IsNullOrWhiteSpace(_region))
            throw new AzureSpeechConfigurationException("Azure Speech is not configured.");

        var voiceName = request.Sex.Trim().ToUpperInvariant() switch
        {
            "M" => _maleVoiceName,
            "F" => _femaleVoiceName,
            _ => throw new ArgumentException("Sex must be M or F.")
        };

        using var speechRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://{_region}.tts.speech.microsoft.com/cognitiveservices/v1");
        speechRequest.Headers.Add("Ocp-Apim-Subscription-Key", _key);
        speechRequest.Headers.Add("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
        speechRequest.Headers.Add("User-Agent", "PF3311-TeleRehab");
        speechRequest.Content = new StringContent(
            BuildSsml(request.Text.Trim(), voiceName),
            Encoding.UTF8,
            "application/ssml+xml");

        using var response = await _httpClient.SendAsync(speechRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AzureSpeechSynthesisException(
                $"Azure Speech returned HTTP {(int)response.StatusCode}.");
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private static string BuildSsml(string text, string voiceName)
    {
        var builder = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true
        };

        using (var writer = XmlWriter.Create(builder, settings))
        {
            writer.WriteStartElement("speak", "http://www.w3.org/2001/10/synthesis");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteAttributeString("xml", "lang", null, "es-CR");
            writer.WriteStartElement("voice");
            writer.WriteAttributeString("name", voiceName);
            writer.WriteString(text);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        return builder.ToString();
    }

    public async Task<string> TranscribeAsync(
        byte[] audioBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audioBytes);

        if (audioBytes.Length == 0)
            throw new ArgumentException("Audio is required.", nameof(audioBytes));

        if (string.IsNullOrWhiteSpace(_key) || string.IsNullOrWhiteSpace(_region))
            throw new AzureSpeechConfigurationException("Azure Speech is not configured.");

        var url =
            $"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=es-CR";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
        request.Headers.Add("Accept", "application/json");
        request.Content = new ByteArrayContent(audioBytes);
        request.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AzureSpeechRecognitionException(
                $"Azure Speech returned HTTP {(int)response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<AzureSpeechRecognitionResponse>(
            cancellationToken);

        if (result is null)
            return string.Empty;

        return string.Equals(result.RecognitionStatus, "Success", StringComparison.OrdinalIgnoreCase)
            ? result.DisplayText
            : string.Empty;
    }
}

public class AzureSpeechConfigurationException : Exception
{
    public AzureSpeechConfigurationException(string message)
        : base(message)
    {
    }
}

public class AzureSpeechSynthesisException : Exception
{
    public AzureSpeechSynthesisException(string message)
        : base(message)
    {
    }
}

public class AzureSpeechRecognitionException : Exception
{
    public AzureSpeechRecognitionException(string message)
        : base(message)
    {
    }
}

public class AzureSpeechRecognitionResponse
{
    public string RecognitionStatus { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public long Offset { get; set; }
    public long Duration { get; set; }
}
