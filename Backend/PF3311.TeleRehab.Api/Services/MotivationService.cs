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
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        string baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        _responsesUrl = $"{baseUrl.TrimEnd('/')}/responses";
    }

    public async Task<string> GenerateMessageAsync(
        MotivationRequest request,
        CancellationToken cancellationToken)
    {
        var (patient, therapy, recentLogs) = GetContext(request);

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OpenAiConfigurationException("OpenAI:ApiKey is not configured.");

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

        using HttpRequestMessage openAiRequest = CreateOpenAiRequest(requestBody);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(openAiRequest, cancellationToken);

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI returned HTTP {(int)response.StatusCode}: {responseJson}");
        }

        return ExtractOutputText(responseJson);
    }

    public async Task<string> GenerateContextMessageAsync(
        ContextMotivationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OpenAiConfigurationException("OpenAI:ApiKey is not configured.");

        var requestBody = new
        {
            model = _model,
            store = false,
            instructions = """
                Eres RehaBot, un asistente virtual de telerehabilitación.

                Genera un mensaje motivacional personalizado en español para un paciente
                que está realizando una terapia de rehabilitación.

                Reglas obligatorias:
                - El mensaje debe ser cálido, humano, empático y motivador.
                - Debe mencionar el nombre del paciente.
                - Debe tomar en cuenta la condición, la terapia, el estado de ánimo y si completó la última terapia.
                - Máximo 80 palabras.
                - No des diagnósticos médicos.
                - No recomiendes medicamentos.
                - No prometas recuperación.
                - Si el paciente indica cansancio, dolor o dificultad, responde con empatía y recomienda avanzar con cuidado.
                - Si hay dolor fuerte o malestar importante, recomienda consultar al profesional de salud.
                - Devuelve únicamente el mensaje final, sin títulos ni explicaciones.
                """,
            input = JsonSerializer.Serialize(new
            {
                patient = new
                {
                    name = request.PatientName,
                    age = request.Age,
                    sex = request.Sex,
                    nationality = request.Nationality,
                    technologyLevel = request.TechnologyLevel,
                    condition = request.Condition
                },
                therapy = new
                {
                    name = request.TherapyName,
                    completedLastTherapy = request.CompletedLastTherapy
                },
                emotionalState = new
                {
                    mood = request.Mood
                }
            }, _jsonOptions)
        };

        using HttpRequestMessage openAiRequest = CreateOpenAiRequest(requestBody);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(openAiRequest, cancellationToken);

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI returned HTTP {(int)response.StatusCode}: {responseJson}");
        }

        return ExtractOutputText(responseJson);
    }

    public async Task<string> GenerateClosingMessageAsync(
        ClosingMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new OpenAiConfigurationException("OpenAI:ApiKey is not configured.");

        var requestBody = new
        {
            model = _model,
            store = false,
            instructions = """
                Eres RehaBot, un asistente virtual de telerehabilitación.

                Debes generar un mensaje de cierre personalizado después de que el paciente realizó
                una terapia y respondió cómo se sintió.

                Reglas obligatorias:
                - Responde siempre en español.
                - El mensaje debe tener al menos 100 palabras.
                - El mensaje debe ser cálido, humano, empático y motivador.
                - Menciona el nombre del paciente.
                - Toma en cuenta la respuesta del paciente.
                - Toma en cuenta la condición y la terapia realizada.
                - Refuerza positivamente el esfuerzo realizado.
                - Si el paciente menciona cansancio, dolor, molestia, dificultad o frustración,
                  responde con empatía y recomienda avanzar con cuidado.
                - Si menciona dolor fuerte, malestar importante o empeoramiento, recomienda consultar
                  al profesional de salud.
                - No des diagnósticos médicos.
                - No recomiendes medicamentos.
                - No prometas recuperación.
                - No presiones al paciente a continuar si tiene dolor.
                - Cierra con una frase motivadora.
                - Devuelve únicamente el mensaje final. No agregues títulos ni explicaciones.
                """,
            input = JsonSerializer.Serialize(new
            {
                patient = new
                {
                    name = request.PatientName,
                    condition = request.Condition
                },
                therapy = new
                {
                    name = request.TherapyName
                },
                interaction = new
                {
                    patientResponse = request.PatientResponse
                }
            }, _jsonOptions)
        };

        using HttpRequestMessage openAiRequest = CreateOpenAiRequest(requestBody);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(openAiRequest, cancellationToken);

        string responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI returned HTTP {(int)response.StatusCode}: {responseJson}");
        }

        return ExtractOutputText(responseJson);
    }

    public string GenerateFallbackMessage(MotivationRequest request)
    {
        var (patient, therapy, recentLogs) = GetContext(request);
        TherapyLog? latestLog = recentLogs.FirstOrDefault();

        if (latestLog is null)
        {
            return $"{patient.FullName}, sigue avanzando con {therapy.Name}. Cada sesión cuenta.";
        }

        if (latestLog.PainLevel >= 4)
        {
            return $"{patient.FullName}, tu avance en {therapy.Name} quedó registrado. "
                + "Como indicaste dolor elevado, pausa el ejercicio y comunícate con tu profesional de salud.";
        }

        if (!latestLog.Completed)
        {
            return $"{patient.FullName}, tu esfuerzo con {therapy.Name} también cuenta. "
                + "Avanza poco a poco y retoma la sesión cuando te sientas preparado.";
        }

        if (latestLog.MoodLevel <= 2)
        {
            return $"{patient.FullName}, completaste {therapy.Name} incluso en un día difícil. "
                + "Reconoce ese avance y continúa a tu ritmo.";
        }

        return $"{patient.FullName}, completaste {therapy.Name}. "
            + "Muy buen trabajo: cada sesión registrada suma a tu proceso de rehabilitación.";
    }

    private HttpRequestMessage CreateOpenAiRequest(object requestBody)
    {
        var openAiRequest = new HttpRequestMessage(HttpMethod.Post, _responsesUrl);

        openAiRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        openAiRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        return openAiRequest;
    }

    private (Patient Patient, Therapy Therapy, IReadOnlyList<TherapyLog> RecentLogs) GetContext(
        MotivationRequest request)
    {
        Patient patient = _database.Patients
            .FirstOrDefault(patient => patient.Id == request.PatientId)
            ?? throw new MotivationContextNotFoundException("Patient not found.");

        Therapy? therapy = _database.Therapies
            .FirstOrDefault(therapy => therapy.Id == request.TherapyId);

        if (therapy is null || therapy.PatientId != patient.Id)
        {
            throw new MotivationContextNotFoundException("Therapy not found for this patient.");
        }

        return (patient, therapy, _database.GetRecentTherapyLogs(patient.Id));
    }

    private static string ExtractOutputText(string responseJson)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(responseJson);

            if (!document.RootElement.TryGetProperty("output", out JsonElement outputs))
            {
                throw new OpenAiMotivationGenerationException(
                    "OpenAI did not return a message.");
            }

            foreach (JsonElement output in outputs.EnumerateArray())
            {
                if (!output.TryGetProperty("content", out JsonElement content))
                    continue;

                foreach (JsonElement item in content.EnumerateArray())
                {
                    if (!item.TryGetProperty("type", out JsonElement type))
                        continue;

                    if (type.GetString() != "output_text")
                        continue;

                    if (!item.TryGetProperty("text", out JsonElement text))
                        continue;

                    string? message = text.GetString();

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message.Trim();
                    }
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

        throw new OpenAiMotivationGenerationException(
            "OpenAI did not return a message.");
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