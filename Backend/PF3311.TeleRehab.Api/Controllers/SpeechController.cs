using Microsoft.AspNetCore.Mvc;
using PF3311.Telerehab.API.Models;
using PF3311.Telerehab.API.Services;

namespace PF3311.Telerehab.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private const int MaximumTextLength = 1000;
    private readonly SpeechService _speechService;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(SpeechService speechService, ILogger<SpeechController> logger)
    {
        _speechService = speechService;
        _logger = logger;
    }

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize(
        [FromBody] SpeechRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > MaximumTextLength)
        {
            return BadRequest(new
            {
                message = $"Text must contain between 1 and {MaximumTextLength} characters."
            });
        }

        if (request.Sex.Trim().ToUpperInvariant() is not ("M" or "F"))
            return BadRequest(new { message = "Sex must be M or F." });

        try
        {
            var audio = await _speechService.SynthesizeAsync(request, cancellationToken);

            return File(audio, "audio/mpeg", "speech.mp3");
        }
        catch (AzureSpeechConfigurationException exception)
        {
            _logger.LogError(exception, "Azure Speech is not configured.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { message = "Speech synthesis is not configured." });
        }
        catch (Exception exception) when (
            exception is HttpRequestException or AzureSpeechSynthesisException)
        {
            _logger.LogWarning(exception, "Azure Speech synthesis failed.");
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "Speech synthesis failed." });
        }
    }

    [HttpPost("transcribe")]
public async Task<IActionResult> Transcribe(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest(new { message = "Audio file is required." });
    }

    await using MemoryStream memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);

    byte[] audioBytes = memoryStream.ToArray();

    string text = await _speechService.TranscribeAsync(audioBytes);

    return Ok(new
    {
        text
    });
}

}
