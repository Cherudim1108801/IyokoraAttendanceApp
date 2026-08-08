using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

public class MemberService
{
    private const string Collection = "members";
    private readonly FirestoreClient _client;

    public MemberService(FirestoreClient client)
    {
        _client = client;
    }

    public async Task<List<Member>> GetAllAsync(CancellationToken ct = default)
    {
        var docs = await _client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId)
            .Select(ToMember)
            .OrderBy(m => m.Part)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public Task SaveAsync(string memberId, string name, PartType part, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["name"] = name,
            ["part"] = part.ToString(),
            ["updatedAt"] = DateTime.UtcNow
        };
        return _client.UpsertDocumentAsync(Collection, memberId, fields, ct);
    }

    private static Member ToMember(FirestoreDocument doc) => new()
    {
        Id = doc.Id,
        Name = doc.GetString("name"),
        Part = Enum.TryParse<PartType>(doc.GetString("part"), out var part) ? part : PartType.Soprano,
        UpdatedAt = doc.GetDateTime("updatedAt")
    };
}
