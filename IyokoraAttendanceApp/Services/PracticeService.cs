using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

public class PracticeService
{
    private const string Collection = "practices";
    private readonly FirestoreClient _client;

    public PracticeService(FirestoreClient client)
    {
        _client = client;
    }

    public async Task<List<Practice>> GetAllAsync(CancellationToken ct = default)
    {
        var docs = await _client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId)
            .Select(ToPractice)
            .OrderBy(p => p.Date)
            .ToList();
    }

    public async Task<Practice?> GetByIdAsync(string practiceId, CancellationToken ct = default)
    {
        var doc = await _client.GetDocumentAsync(Collection, practiceId, ct);
        return doc is null ? null : ToPractice(doc);
    }

    public async Task<Practice?> GetNextUpcomingAsync(CancellationToken ct = default)
    {
        var all = await GetAllAsync(ct);
        var today = DateTime.Today;
        return all.Where(p => p.Date.Date >= today).OrderBy(p => p.Date).FirstOrDefault();
    }

    public async Task<string> CreateAsync(DateTime date, string title, string place, CancellationToken ct = default)
    {
        var id = Guid.NewGuid().ToString("N");
        // 練習日は時刻を持たないカレンダー日付として扱う。DateTime.ToUniversalTime() による
        // タイムゾーン変換で日付がずれないよう、日付部分だけを UTC として保存する。
        var dateOnly = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["date"] = dateOnly,
            ["title"] = title,
            ["place"] = place,
            ["createdAt"] = DateTime.UtcNow
        };
        await _client.UpsertDocumentAsync(Collection, id, fields, ct);
        return id;
    }

    public Task DeleteAsync(string practiceId, CancellationToken ct = default) =>
        _client.DeleteDocumentAsync(Collection, practiceId, ct);

    private static Practice ToPractice(FirestoreDocument doc) => new()
    {
        Id = doc.Id,
        Date = doc.GetDateTime("date"),
        Title = doc.GetString("title"),
        Place = doc.GetString("place"),
        CreatedAt = doc.GetDateTime("createdAt")
    };
}
