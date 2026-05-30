using System.Text.Json;
using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Data;

public class InMemoryDatabase
{
    private readonly object _sync = new();
    private readonly string _filePath;
    private readonly TimeSpan _itemLifetime;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private DatabaseState _state;

    public InMemoryDatabase(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["DataStore:FilePath"] ?? "Data/tele-rehab-data.json";
        _filePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);

        var lifetimeHours = configuration.GetValue("DataStore:ItemLifetimeHours", 1);
        _itemLifetime = TimeSpan.FromHours(Math.Max(1, lifetimeHours));
        _state = Load();
        CleanupExpiredData();
    }

    public IReadOnlyList<Patient> Patients => GetValues(_state.Patients);
    public IReadOnlyList<Therapy> Therapies => GetValues(_state.Therapies);
    public IReadOnlyList<TherapyLog> TherapyLogs => GetValues(_state.TherapyLogs);

    public void AddPatient(Patient patient) => Add(_state.Patients, patient);
    public void AddTherapy(Therapy therapy) => Add(_state.Therapies, therapy);
    public void AddTherapyLog(TherapyLog log) => Add(_state.TherapyLogs, log);

    public void UpdatePatient(Patient patient) => Refresh(_state.Patients, item => item.Value.Id == patient.Id);
    public void UpdateTherapy(Therapy therapy) => Refresh(_state.Therapies, item => item.Value.Id == therapy.Id);

    public bool RemovePatient(Guid id) => Remove(_state.Patients, item => item.Value.Id == id);
    public bool RemoveTherapy(Guid id) => Remove(_state.Therapies, item => item.Value.Id == id);
    public bool RemoveTherapyLog(Guid id) => Remove(_state.TherapyLogs, item => item.Value.Id == id);

    public void CleanupExpiredData()
    {
        lock (_sync)
        {
            var utcNow = DateTime.UtcNow;
            var removed = _state.Patients.RemoveAll(item => item.IsExpired(utcNow))
                + _state.Therapies.RemoveAll(item => item.IsExpired(utcNow))
                + _state.TherapyLogs.RemoveAll(item => item.IsExpired(utcNow));

            if (removed > 0)
                Save();
        }
    }

    private IReadOnlyList<T> GetValues<T>(List<InMemoryItem<T>> items)
    {
        CleanupExpiredData();

        lock (_sync)
        {
            return items.Select(item => item.Value).ToList();
        }
    }

    private void Add<T>(List<InMemoryItem<T>> items, T value)
    {
        lock (_sync)
        {
            items.Add(new InMemoryItem<T>(value, _itemLifetime));
            Save();
        }
    }

    private void Refresh<T>(List<InMemoryItem<T>> items, Func<InMemoryItem<T>, bool> predicate)
    {
        lock (_sync)
        {
            var item = items.First(predicate);
            item.RefreshExpiration(_itemLifetime);
            Save();
        }
    }

    private bool Remove<T>(List<InMemoryItem<T>> items, Func<InMemoryItem<T>, bool> predicate)
    {
        lock (_sync)
        {
            var item = items.FirstOrDefault(predicate);

            if (item is null)
                return false;

            items.Remove(item);
            Save();
            return true;
        }
    }

    private DatabaseState Load()
    {
        if (!File.Exists(_filePath))
            return new DatabaseState();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<DatabaseState>(json, _jsonOptions) ?? new DatabaseState();
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(_state, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class DatabaseState
    {
        public List<InMemoryItem<Patient>> Patients { get; set; } = new();
        public List<InMemoryItem<Therapy>> Therapies { get; set; } = new();
        public List<InMemoryItem<TherapyLog>> TherapyLogs { get; set; } = new();
    }
}
