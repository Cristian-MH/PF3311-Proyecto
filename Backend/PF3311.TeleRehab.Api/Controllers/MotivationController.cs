using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Models;
using PF3311.Telerehab.API.Services;

namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotivationController : ControllerBase
{
    private readonly MotivationService _motivationService;
    private readonly ILogger<MotivationController> _logger;

    public MotivationController(
        MotivationService motivationService,
        ILogger<MotivationController> logger)
    {
        _motivationService = motivationService;
        _logger = logger;
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
            _logger.LogWarning(exception, "OpenAI is not configured. Using fallback motivation message.");

            return Ok(new
            {
                message = _motivationService.GenerateFallbackMessage(request),
                generatedByAi = false
            });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or OpenAiMotivationGenerationException)
        {
            _logger.LogWarning(exception, "OpenAI motivation generation failed. Using fallback message.");

            return Ok(new
            {
                message = _motivationService.GenerateFallbackMessage(request),
                generatedByAi = false
            });
        }
    }

    [HttpPost("closing-message")]
    public async Task<IActionResult> GenerateClosingMessage(
    [FromBody] ClosingMessageRequest request,
    CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Request body is required." });
        }

        if (string.IsNullOrWhiteSpace(request.PatientResponse))
        {
            return BadRequest(new { message = "Patient response is required." });
        }

        try
        {
            string message = await _motivationService.GenerateClosingMessageAsync(
                request,
                cancellationToken);

            return Ok(new ClosingMessageResponse
            {
                Message = message
            });
        }
        catch (OpenAiConfigurationException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "OpenAI is not configured." });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or OpenAiMotivationGenerationException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Closing message generation failed." });
        }
    }
}
    