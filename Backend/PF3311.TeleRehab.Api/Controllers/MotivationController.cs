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
    public async Task<IActionResult> GenerateMessage(
        [FromBody] MotivationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _motivationService.GenerateMessageAsync(request, cancellationToken);

            return Ok(new
            {
                message
            });
        }
        catch (MotivationContextNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (OpenAiConfigurationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or OpenAiMotivationGenerationException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "OpenAI could not generate the motivation message."
            });
        }
    }
}
