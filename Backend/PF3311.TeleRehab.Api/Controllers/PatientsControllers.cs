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

    [HttpPost]
    public IActionResult Create([FromBody] Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FullName))
            return BadRequest(new { message = "FullName is required." });

        if (patient.Age < 1 || patient.Age > 120)
            return BadRequest(new { message = "Age must be between 1 and 120." });

        if (string.IsNullOrWhiteSpace(patient.Condition))
            return BadRequest(new { message = "Condition is required." });

        patient.Id = Guid.NewGuid();

        _database.AddPatient(patient);

        return StatusCode(StatusCodes.Status201Created, patient);
    }
}
