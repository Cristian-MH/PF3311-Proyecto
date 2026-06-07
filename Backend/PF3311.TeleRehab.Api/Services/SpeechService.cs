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

    public async Task<SpeechTranscriptionResult> TranscribeAsync(byte[] audioBytes)
    {
        if (audioBytes == null || audioBytes.Length == 0)
        {
            throw new ArgumentException("Audio is required.", nameof(audioBytes));
        }

        string language = "es-CR";

        string url =
            $"https://{_region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language={language}&format=simple";

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
        request.Headers.Add("Accept", "application/json");

        request.Content = new ByteArrayContent(audioBytes);
        request.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

        using HttpResponseMessage response = await _httpClient.SendAsync(request);

        string responseBody = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Azure STT raw response:");
        Console.WriteLine(responseBody);

        if (!response.IsSuccessStatusCode)
        {
            return new SpeechTranscriptionResult
            {
                Text = string.Empty,
                RecognitionStatus = "HttpError",
                RawResponse = responseBody,
                Error = $"Azure Speech STT error {(int)response.StatusCode}"
            };
        }

        AzureSpeechRecognitionResponse? azureResult =
            System.Text.Json.JsonSerializer.Deserialize<AzureSpeechRecognitionResponse>(
                responseBody,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

        if (azureResult == null)
        {
            return new SpeechTranscriptionResult
            {
                Text = string.Empty,
                RecognitionStatus = "InvalidResponse",
                RawResponse = responseBody
            };
        }

        return new SpeechTranscriptionResult
        {
            Text = azureResult.DisplayText ?? string.Empty,
            RecognitionStatus = azureResult.RecognitionStatus ?? string.Empty,
            RawResponse = responseBody
        };
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

public class SpeechTranscriptionResult
{
    public string Text { get; set; } = string.Empty;
    public string RecognitionStatus { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class AzureSpeechRecognitionResponse
{
    public string RecognitionStatus { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public long Offset { get; set; }
    public long Duration { get; set; }
}