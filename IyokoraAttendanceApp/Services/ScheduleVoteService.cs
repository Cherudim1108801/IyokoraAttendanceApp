using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

/// <summary>Firestore の <c>scheduleVotes</c> コレクションに対する日程投票の参加意思の取得・更新を担う。</summary>
public class ScheduleVoteService(FirestoreClient client)
{
    private const string Collection = "scheduleVotes";

    /// <summary>指定候補日に対する投票を取得する。</summary>
    /// <param name="candidateId">候補日ID。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public async Task<List<ScheduleVote>> GetForCandidateAsync(string candidateId, CancellationToken ct = default)
    {
        var docs = await client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId && d.GetString("candidateId") == candidateId)
            .Select(ToVote)
            .ToList();
    }

    /// <summary>指定した複数の候補日に対する投票をまとめて取得する。</summary>
    /// <param name="candidateIds">候補日IDの一覧。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public async Task<List<ScheduleVote>> GetForCandidatesAsync(IEnumerable<string> candidateIds, CancellationToken ct = default)
    {
        var ids = candidateIds.ToHashSet();
        var docs = await client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId && ids.Contains(d.GetString("candidateId")))
            .Select(ToVote)
            .ToList();
    }

    /// <summary>指定候補日への参加意思を登録・変更する。</summary>
    /// <param name="candidateId">候補日ID。</param>
    /// <param name="memberId">投票するメンバーのID。</param>
    /// <param name="memberName">投票するメンバーの表示名。</param>
    /// <param name="status">参加意思。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public Task SetStatusAsync(string candidateId, string memberId, string memberName, AttendanceStatus status, CancellationToken ct = default)
    {
        var id = ScheduleVote.BuildId(candidateId, memberId);
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["candidateId"] = candidateId,
            ["memberId"] = memberId,
            ["memberName"] = memberName,
            ["status"] = status.ToString(),
            ["updatedAt"] = DateTime.UtcNow
        };
        return client.UpsertDocumentAsync(Collection, id, fields, ct);
    }

    /// <summary>指定候補日に対して登録済みの投票をすべて削除する。</summary>
    /// <param name="candidateId">候補日ID。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public async Task DeleteForCandidateAsync(string candidateId, CancellationToken ct = default)
    {
        var votes = await GetForCandidateAsync(candidateId, ct);
        foreach (var vote in votes)
            await client.DeleteDocumentAsync(Collection, vote.Id, ct);
    }

    private static ScheduleVote ToVote(FirestoreDocument doc) => new()
    {
        Id = doc.Id,
        CandidateId = doc.GetString("candidateId"),
        MemberId = doc.GetString("memberId"),
        MemberName = doc.GetString("memberName"),
        Status = Enum.TryParse<AttendanceStatus>(doc.GetString("status"), out var status) ? status : AttendanceStatus.Undecided,
        UpdatedAt = doc.GetDateTime("updatedAt")
    };
}
