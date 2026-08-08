using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

public class AttendanceService
{
    private const string Collection = "attendances";
    private readonly FirestoreClient _client;

    public AttendanceService(FirestoreClient client)
    {
        _client = client;
    }

    public async Task<List<Attendance>> GetForPracticeAsync(string practiceId, CancellationToken ct = default)
    {
        var docs = await _client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId && d.GetString("practiceId") == practiceId)
            .Select(ToAttendance)
            .ToList();
    }

    public async Task<Attendance?> GetForMemberAsync(string practiceId, string memberId, CancellationToken ct = default)
    {
        var all = await GetForPracticeAsync(practiceId, ct);
        return all.FirstOrDefault(a => a.MemberId == memberId);
    }

    public Task SetStatusAsync(string practiceId, string memberId, string memberName, PartType part, AttendanceStatus status, CancellationToken ct = default)
    {
        var id = Attendance.BuildId(practiceId, memberId);
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["practiceId"] = practiceId,
            ["memberId"] = memberId,
            ["memberName"] = memberName,
            ["part"] = part.ToString(),
            ["status"] = status.ToString(),
            ["updatedAt"] = DateTime.UtcNow
        };
        return _client.UpsertDocumentAsync(Collection, id, fields, ct);
    }

    private static Attendance ToAttendance(FirestoreDocument doc) => new()
    {
        Id = doc.Id,
        PracticeId = doc.GetString("practiceId"),
        MemberId = doc.GetString("memberId"),
        MemberName = doc.GetString("memberName"),
        Part = Enum.TryParse<PartType>(doc.GetString("part"), out var part) ? part : PartType.Soprano,
        Status = Enum.TryParse<AttendanceStatus>(doc.GetString("status"), out var status) ? status : AttendanceStatus.Undecided,
        UpdatedAt = doc.GetDateTime("updatedAt")
    };
}
