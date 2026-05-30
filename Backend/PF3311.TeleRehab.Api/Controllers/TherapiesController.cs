using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TherapiesController : ControllerBase
{
    private readonly InMemoryDatabase _database;

    public TherapiesController(InMemoryDatabase database)
    {
        _database = database;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_database.Therapies);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var therapy = _database.Therapies.FirstOrDefault(t => t.Id == id);

        if (therapy is null)
            return NotFound(new { message = "Therapy not found." });

        return Ok(therapy);
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

    [HttpPost]
    public IActionResult Create([FromBody] Therapy therapy)
    {
        var patientExists = _database.Patients.Any(p => p.Id == therapy.PatientId);

        if (!patientExists)
            return BadRequest(new { message = "Invalid PatientId. Patient does not exist." });

        if (string.IsNullOrWhiteSpace(therapy.Name))
            return BadRequest(new { message = "Name is required." });

        therapy.Id = Guid.NewGuid();

        _database.Therapies.Add(therapy);

        return CreatedAtAction(nameof(GetById), new { id = therapy.Id }, therapy);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Therapy updatedTherapy)
    {
        var therapy = _database.Therapies.FirstOrDefault(t => t.Id == id);

        if (therapy is null)
            return NotFound(new { message = "Therapy not found." });

        var patientExists = _database.Patients.Any(p => p.Id == updatedTherapy.PatientId);

        if (!patientExists)
            return BadRequest(new { message = "Invalid PatientId. Patient does not exist." });

        therapy.PatientId = updatedTherapy.PatientId;
        therapy.Name = updatedTherapy.Name;
        therapy.Instructions = updatedTherapy.Instructions;
        therapy.Repetitions = updatedTherapy.Repetitions;
        therapy.Frequency = updatedTherapy.Frequency;

        return Ok(therapy);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var therapy = _database.Therapies.FirstOrDefault(t => t.Id == id);

        if (therapy is null)
            return NotFound(new { message = "Therapy not found." });

        _database.Therapies.Remove(therapy);

        return NoContent();
    }
}