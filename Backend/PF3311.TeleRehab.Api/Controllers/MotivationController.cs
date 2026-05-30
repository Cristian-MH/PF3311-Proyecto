using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Models;
using PF3311.Telerehab.API.Services;

namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotivationController : ControllerBase
{
    private readonly MotivationService _motivationService;

    public MotivationController(MotivationService motivationService)
    {
        _motivationService = motivationService;
    }

    [HttpPost("message")]
    public IActionResult GenerateMessage([FromBody] MotivationRequest request)
    {
        var message = _motivationService.GenerateMessage(request);

        return Ok(new
        {
            message
        });
    }
}