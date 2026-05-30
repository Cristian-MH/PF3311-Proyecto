namespace PF3311.Telerehab.API.Models;

public class TherapyLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid TherapyId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public bool Completed { get; set; }
    public int MoodLevel { get; set; }
    public int PainLevel { get; set; }
    public string Comment { get; set; } = string.Empty;
}