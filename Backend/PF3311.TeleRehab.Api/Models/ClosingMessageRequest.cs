namespace PF3311.Telerehab.API.Models;

public class ClosingMessageRequest
{
    public Guid PatientId { get; set; }
    public Guid TherapyId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientResponse { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string TherapyName { get; set; } = string.Empty;
}
