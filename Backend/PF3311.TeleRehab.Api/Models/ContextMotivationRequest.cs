namespace PF3311.Telerehab.API.Models;

public class ContextMotivationRequest
{
    public Guid PatientId { get; set; }
    public Guid TherapyId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string TechnologyLevel { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string TherapyName { get; set; } = string.Empty;
    public string Mood { get; set; } = string.Empty;
    public bool CompletedLastTherapy { get; set; }
}

public class ContextMotivationResponse
{
    public string Message { get; set; } = string.Empty;
}
