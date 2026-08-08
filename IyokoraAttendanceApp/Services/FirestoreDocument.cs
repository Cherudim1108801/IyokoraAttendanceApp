namespace IyokoraAttendanceApp.Services;

public class FirestoreDocument
{
    public required string Id { get; init; }
    public required Dictionary<string, object?> Fields { get; init; }

    public string GetString(string key, string fallback = "") =>
        Fields.TryGetValue(key, out var v) && v is string s ? s : fallback;

    public long GetLong(string key, long fallback = 0) =>
        Fields.TryGetValue(key, out var v) && v is long l ? l : fallback;

    public DateTime GetDateTime(string key, DateTime fallback = default) =>
        Fields.TryGetValue(key, out var v) && v is DateTime dt ? dt : fallback;
}
