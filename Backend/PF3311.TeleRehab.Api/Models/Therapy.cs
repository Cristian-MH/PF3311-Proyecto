namespace PF3311.Telerehab.API.Models;

public class Therapy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Repetitions { get; set; }
    public string Frequency { get; set; } = string.Empty;
}