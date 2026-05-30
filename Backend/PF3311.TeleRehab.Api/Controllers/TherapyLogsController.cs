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

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_database.TherapyLogs);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var log = _database.TherapyLogs.FirstOrDefault(l => l.Id == id);

        if (log is null)
            return NotFound(new { message = "Therapy log not found." });

        return Ok(log);
    }

    [HttpGet("patient/{patientId:guid}")]
    public IActionResult GetByPatientId(Guid patientId)
    {
        var patientExists = _database.Patients.Any(p => p.Id == patientId);

        if (!patientExists)
            return NotFound(new { message = "Patient not found." });

        var logs = _database.TherapyLogs
            .Where(l => l.PatientId == patientId)
            .OrderByDescending(l => l.CompletedAt)
            .ToList();

        return Ok(logs);
    }

    [HttpGet("therapy/{therapyId:guid}")]
    public IActionResult GetByTherapyId(Guid therapyId)
    {
        var therapyExists = _database.Therapies.Any(t => t.Id == therapyId);

        if (!therapyExists)
            return NotFound(new { message = "Therapy not found." });

        var logs = _database.TherapyLogs
            .Where(l => l.TherapyId == therapyId)
            .OrderByDescending(l => l.CompletedAt)
            .ToList();

        return Ok(logs);
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

        _database.TherapyLogs.Add(log);

        return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var log = _database.TherapyLogs.FirstOrDefault(l => l.Id == id);

        if (log is null)
            return NotFound(new { message = "Therapy log not found." });

        _database.TherapyLogs.Remove(log);

        return NoContent();
    }
}