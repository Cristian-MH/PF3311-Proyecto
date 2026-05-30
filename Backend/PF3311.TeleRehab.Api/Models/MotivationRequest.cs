namespace PF3311.Telerehab.API.Models;

public class MotivationRequest
{
    public string PatientName { get; set; } = string.Empty;
    public string TherapyName { get; set; } = string.Empty;
    public string Mood { get; set; } = string.Empty;
    public bool CompletedLastTherapy { get; set; }
}
