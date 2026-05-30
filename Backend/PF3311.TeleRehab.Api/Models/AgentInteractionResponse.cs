namespace PF3311.Telerehab.API.Models;
public class AgentInteractionResponse
{
    public string Message { get; set; } = string.Empty;
    public string Emotion { get; set; } = "neutral";
    public string Animation { get; set; } = "talk";
    public bool ShouldRegisterLog { get; set; }
}