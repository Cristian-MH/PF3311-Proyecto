namespace PF3311.Telerehab.API.Models;

public class Patient
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string TechnologyLevel { get; set; } = string.Empty;
}
