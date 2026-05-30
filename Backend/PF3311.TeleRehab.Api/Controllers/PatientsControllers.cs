using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Data;
using PF3311.Telerehab.API.Models;
namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly InMemoryDatabase _database;

    public PatientsController(InMemoryDatabase database)
    {
        _database = database;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_database.Patients);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        var patient = _database.Patients.FirstOrDefault(p => p.Id == id);

        if (patient is null)
            return NotFound(new { message = "Patient not found." });

        return Ok(patient);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
            return BadRequest(new { message = "FullName is required." });

        patient.Id = Guid.NewGuid();

        _database.Patients.Add(patient);

        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Patient updatedPatient)
    {
        var patient = _database.Patients.FirstOrDefault(p => p.Id == id);

        if (patient is null)
            return NotFound(new { message = "Patient not found." });

        patient.FullName = updatedPatient.FullName;
        patient.Age = updatedPatient.Age;
        patient.Sex = updatedPatient.Sex;
        patient.Condition = updatedPatient.Condition;
        patient.TechnologyLevel = updatedPatient.TechnologyLevel;

        return Ok(patient);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var patient = _database.Patients.FirstOrDefault(p => p.Id == id);

        if (patient is null)
            return NotFound(new { message = "Patient not found." });

        _database.Patients.Remove(patient);

        return NoContent();
    }
}