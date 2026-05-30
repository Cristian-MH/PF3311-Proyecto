using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Services;

public class OpenAiTherapyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _responsesUrl;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public OpenAiTherapyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:Model"] ?? "gpt-5.4-mini";
        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        _responsesUrl = $"{baseUrl.TrimEnd('/')}/responses";
    }

    public async Task<IReadOnlyList<Therapy>> GenerateAsync(
        Patient patient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OpenAiConfigurationException("OpenAI:ApiKey is not configured.");

        var requestBody = new
        {
            model = _model,
            store = false,
            instructions = """
                You generate a conservative telerehabilitation exercise plan in Spanish.
                Use only the supplied patient context. Do not diagnose, prescribe medication,
                or claim that the plan replaces a clinician. Return between 5 and 7 simple,
                low-risk exercises suitable for remote guidance. Each instruction must be clear,
                mention stopping if pain increases, and recommend clinician review before use.
                """,
            input = JsonSerializer.Serialize(new
            {
                patient.Age,
                patient.Sex,
                patient.Condition,
                patient.TechnologyLevel
            }, _jsonOptions),
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "telerehabilitation_plan",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            therapies = new
                            {
                                type = "array",
                                minItems = 5,
                                maxItems = 7,
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        name = new { type = "string" },
                                        instructions = new { type = "string" },
                                        repetitions = new { type = "integer", minimum = 1 },
                                        frequency = new { type = "string" }
                                    },
                                    required = new[] { "name", "instructions", "repetitions", "frequency" },
                                    additionalProperties = false
                                }
                            }
                        },
                        required = new[] { "therapies" },
                        additionalProperties = false
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _responsesUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenAI returned HTTP {(int)response.StatusCode}.");

        GeneratedTherapyPlan? plan;

        try
        {
            var outputText = ExtractOutputText(responseJson);
            plan = JsonSerializer.Deserialize<GeneratedTherapyPlan>(outputText, _jsonOptions);
        }
        catch (JsonException exception)
        {
            throw new OpenAiTherapyGenerationException(
                "OpenAI returned an invalid therapy plan.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new OpenAiTherapyGenerationException(
                "OpenAI returned an invalid therapy plan.",
                exception);
        }

        if (plan?.Therapies is null
            || plan.Therapies.Count < 5
            || plan.Therapies.Any(exercise =>
                string.IsNullOrWhiteSpace(exercise.Name)
                || string.IsNullOrWhiteSpace(exercise.Instructions)
                || exercise.Repetitions < 1
                || string.IsNullOrWhiteSpace(exercise.Frequency)))
        {
            throw new OpenAiTherapyGenerationException("OpenAI returned an invalid therapy plan.");
        }

        return plan.Therapies.Select(exercise => new Therapy
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Name = exercise.Name,
            Instructions = exercise.Instructions,
            Repetitions = exercise.Repetitions,
            Frequency = exercise.Frequency
        }).ToList();
    }

    private static string ExtractOutputText(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty("output", out var outputs))
            throw new OpenAiTherapyGenerationException("OpenAI did not return a therapy plan.");

        foreach (var output in outputs.EnumerateArray())
        {
            if (!output.TryGetProperty("content", out var content))
                continue;

            foreach (var item in content.EnumerateArray())
            {
                if (item.GetProperty("type").GetString() == "output_text")
                    return item.GetProperty("text").GetString() ?? string.Empty;
            }
        }

        throw new OpenAiTherapyGenerationException("OpenAI did not return a therapy plan.");
    }

    private sealed class GeneratedTherapyPlan
    {
        public List<GeneratedTherapy> Therapies { get; set; } = new();
    }

    private sealed class GeneratedTherapy
    {
        public string Name { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public int Repetitions { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }
}

public class OpenAiConfigurationException : Exception
{
    public OpenAiConfigurationException(string message)
        : base(message)
    {
    }
}

public class OpenAiTherapyGenerationException : Exception
{
    public OpenAiTherapyGenerationException(string message)
        : base(message)
    {
    }

    public OpenAiTherapyGenerationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
