using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Services;

public class MotivationService
{
    private readonly HttpClient _httpClient;
    private readonly InMemoryDatabase _database;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _responsesUrl;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public MotivationService(
        HttpClient httpClient,
        InMemoryDatabase database,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _database = database;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:Model"] ?? "gpt-5.4-mini";
        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        _responsesUrl = $"{baseUrl.TrimEnd('/')}/responses";
    }

    public async Task<string> GenerateMessageAsync(
        MotivationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OpenAiConfigurationException("OpenAI:ApiKey is not configured.");

        var patient = _database.Patients.FirstOrDefault(patient => patient.Id == request.PatientId)
            ?? throw new MotivationContextNotFoundException("Patient not found.");
        var therapy = _database.Therapies.FirstOrDefault(therapy => therapy.Id == request.TherapyId);

        if (therapy is null || therapy.PatientId != patient.Id)
            throw new MotivationContextNotFoundException("Therapy not found for this patient.");

        var recentLogs = _database.GetRecentTherapyLogs(patient.Id);
        var requestBody = new
        {
            model = _model,
            store = false,
            instructions = """
                Generate one personalized motivational message in Spanish for a telerehabilitation
                patient. Use the supplied patient, exercise, and recent progress context. Be warm,
                concise, and realistic. Acknowledge effort and adapt the tone when pain is elevated
                or a session was not completed. Do not diagnose, prescribe medication, promise
                recovery, or pressure the patient to continue through pain. When pain is elevated,
                recommend pausing and contacting the clinician. Return only the message, with a
                maximum of 80 words.
                """,
            input = JsonSerializer.Serialize(new
            {
                patient = new
                {
                    patient.FullName,
                    patient.Age,
                    patient.Condition
                },
                therapy = new
                {
                    therapy.Name,
                    therapy.Frequency
                },
                recentProgress = recentLogs.Select(log => new
                {
                    log.CompletedAt,
                    log.Completed,
                    log.MoodLevel,
                    log.PainLevel,
                    log.Comment
                })
            }, _jsonOptions)
        };

        using var openAiRequest = new HttpRequestMessage(HttpMethod.Post, _responsesUrl);
        openAiRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        openAiRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(openAiRequest, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenAI returned HTTP {(int)response.StatusCode}.");

        return ExtractOutputText(responseJson);
    }

    private static string ExtractOutputText(string responseJson)
    {
        try
        {
            using var document = JsonDocument.Parse(responseJson);

            if (!document.RootElement.TryGetProperty("output", out var outputs))
                throw new OpenAiMotivationGenerationException("OpenAI did not return a message.");

            foreach (var output in outputs.EnumerateArray())
            {
                if (!output.TryGetProperty("content", out var content))
                    continue;

                foreach (var item in content.EnumerateArray())
                {
                    if (item.GetProperty("type").GetString() != "output_text")
                        continue;

                    var message = item.GetProperty("text").GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                        return message.Trim();
                }
            }
        }
        catch (JsonException exception)
        {
            throw new OpenAiMotivationGenerationException(
                "OpenAI returned an invalid motivation message.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new OpenAiMotivationGenerationException(
                "OpenAI returned an invalid motivation message.",
                exception);
        }

        throw new OpenAiMotivationGenerationException("OpenAI did not return a message.");
    }
}

public class MotivationContextNotFoundException : Exception
{
    public MotivationContextNotFoundException(string message)
        : base(message)
    {
    }
}

public class OpenAiMotivationGenerationException : Exception
{
    public OpenAiMotivationGenerationException(string message)
        : base(message)
    {
    }

    public OpenAiMotivationGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
