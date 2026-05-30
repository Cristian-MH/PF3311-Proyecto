using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;

namespace TeleRehab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly InMemoryDatabase _database;

    public AgentController(InMemoryDatabase database)
    {
        _database = database;
    }

    [HttpPost("interact")]
    public IActionResult Interact([FromBody] AgentInteractionRequest request)
    {
        var patient = _database.Patients.FirstOrDefault(p => p.Id == request.PatientId);

        if (patient is null)
            return NotFound(new { message = "Patient not found." });

        Therapy? therapy = null;

        if (request.TherapyId.HasValue)
        {
            therapy = _database.Therapies.FirstOrDefault(t => t.Id == request.TherapyId.Value);

            if (therapy is null)
                return NotFound(new { message = "Therapy not found." });
        }

        var response = BuildSimpleAgentResponse(patient, therapy, request);

        return Ok(response);
    }

    private static AgentInteractionResponse BuildSimpleAgentResponse(
        Patient patient,
        Therapy? therapy,
        AgentInteractionRequest request)
    {
        var therapyName = therapy?.Name ?? "tu terapia";
        var userMessage = request.UserMessage.ToLower();

        if (userMessage.Contains("cansado") || request.MoodLevel <= 2)
        {
            return new AgentInteractionResponse
            {
                Message = $"{patient.FullName}, entiendo que hoy te sientas cansado. Podemos avanzar poco a poco con {therapyName}. Lo importante es mantener la constancia.",
                Emotion = "empathetic",
                Animation = "listen_to_talk",
                ShouldRegisterLog = false
            };
        }

        if (userMessage.Contains("terminé") || userMessage.Contains("complete") || userMessage.Contains("completé"))
        {
            return new AgentInteractionResponse
            {
                Message = $"Excelente trabajo, {patient.FullName}. Registraré tu avance en {therapyName}. Cada sesión completada cuenta para tu recuperación.",
                Emotion = "happy",
                Animation = "celebrate",
                ShouldRegisterLog = true
            };
        }

        return new AgentInteractionResponse
        {
            Message = $"{patient.FullName}, recuerda realizar {therapyName}. Estoy aquí para acompañarte durante la sesión.",
            Emotion = "neutral",
            Animation = "talk",
            ShouldRegisterLog = false
        };
    }
}