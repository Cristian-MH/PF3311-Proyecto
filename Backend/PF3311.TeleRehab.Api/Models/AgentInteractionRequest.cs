namespace PF3311.Telerehab.API.Models;

public class AgentInteractionRequest
{
    public Guid PatientId { get; set; }
    public Guid? TherapyId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public int MoodLevel { get; set; }
    public int PainLevel { get; set; }
}