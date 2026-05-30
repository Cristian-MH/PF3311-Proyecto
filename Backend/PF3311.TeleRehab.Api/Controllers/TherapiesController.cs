using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;
using PF3311.Telerehab.API.Services;

namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TherapiesController : ControllerBase
{
    private readonly InMemoryDatabase _database;
    private readonly OpenAiTherapyService _therapyService;

    public TherapiesController(InMemoryDatabase database, OpenAiTherapyService therapyService)
    {
        _database = database;
        _therapyService = therapyService;
    }

    [HttpGet("patient/{patientId:guid}")]
    public IActionResult GetByPatientId(Guid patientId)
    {
        var patientExists = _database.Patients.Any(p => p.Id == patientId);

        if (!patientExists)
            return NotFound(new { message = "Patient not found." });

        var therapies = _database.Therapies
            .Where(t => t.PatientId == patientId)
            .ToList();

        return Ok(therapies);
    }

    [HttpPost("generate/{patientId:guid}")]
    public async Task<IActionResult> Generate(Guid patientId, CancellationToken cancellationToken)
    {
        var patient = _database.Patients.FirstOrDefault(p => p.Id == patientId);

        if (patient is null)
            return NotFound(new { message = "Patient not found." });

        try
        {
            var therapies = await _therapyService.GenerateAsync(patient, cancellationToken);
            _database.AddTherapies(therapies);

            return StatusCode(StatusCodes.Status201Created, therapies);
        }
        catch (OpenAiConfigurationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or OpenAiTherapyGenerationException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "OpenAI could not generate the therapy plan."
            });
        }
    }
}
