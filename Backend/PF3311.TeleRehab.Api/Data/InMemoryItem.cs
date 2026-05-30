namespace PF3311.Telerehab.API.Data;

public class InMemoryItem<T>
{
    public InMemoryItem()
    {
    }

    public InMemoryItem(T value, TimeSpan lifetime)
    {
        Value = value;
        RefreshExpiration(lifetime);
    }

    public T Value { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }

    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;

    public void RefreshExpiration(TimeSpan lifetime)
    {
        ExpiresAtUtc = DateTime.UtcNow.Add(lifetime);
    }
}
