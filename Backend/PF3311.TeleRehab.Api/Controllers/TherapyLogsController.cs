using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;
    
namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TherapyLogsController : ControllerBase
{
    private readonly InMemoryDatabase _database;

    public TherapyLogsController(InMemoryDatabase database)
    {
        _database = database;
    }

    [HttpPost]
    public IActionResult Create([FromBody] TherapyLog log)
    {
        var patientExists = _database.Patients.Any(p => p.Id == log.PatientId);

        if (!patientExists)
            return BadRequest(new { message = "Invalid PatientId. Patient does not exist." });

        var therapyExists = _database.Therapies.Any(t => t.Id == log.TherapyId);

        if (!therapyExists)
            return BadRequest(new { message = "Invalid TherapyId. Therapy does not exist." });

        if (log.MoodLevel < 1 || log.MoodLevel > 5)
            return BadRequest(new { message = "MoodLevel must be between 1 and 5." });

        if (log.PainLevel < 1 || log.PainLevel > 5)
            return BadRequest(new { message = "PainLevel must be between 1 and 5." });

        log.Id = Guid.NewGuid();
        log.CompletedAt = DateTime.UtcNow;

        _database.AddTherapyLog(log);

        return StatusCode(StatusCodes.Status201Created, log);
    }
}
