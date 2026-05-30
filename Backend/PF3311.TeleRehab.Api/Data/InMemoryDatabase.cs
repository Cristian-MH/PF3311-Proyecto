using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Data;

public class InMemoryDatabase
{
    public List<Patient> Patients { get; } = new();
    public List<Therapy> Therapies { get; } = new();
    public List<TherapyLog> TherapyLogs { get; } = new();
}