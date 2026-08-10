using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

/// <summary>Firestore の <c>members</c> コレクションに対するメンバー情報の取得・保存を担う。</summary>
public class MemberService(FirestoreClient client)
{
    private const string Collection = "members";

    /// <summary>登録されている全メンバーを、パート → 氏名の順で取得する。</summary>
    /// <param name="ct">キャンセルトークン。</param>
    public async Task<List<Member>> GetAllAsync(CancellationToken ct = default)
    {
        var docs = await client.ListDocumentsAsync(Collection, ct);
        return docs
            .Where(d => d.GetString("groupId") == FirebaseOptions.GroupId)
            .Select(ToMember)
            .OrderBy(m => m.Part)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>メンバーの名前・パートを新規登録または更新する。</summary>
    /// <param name="memberId">端末で発行された MemberId。</param>
    /// <param name="name">表示名。</param>
    /// <param name="part">所属パート。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public Task SaveAsync(string memberId, string name, PartType part, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["name"] = name,
            ["part"] = part.ToString(),
            ["updatedAt"] = DateTime.UtcNow
        };
        return client.UpsertDocumentAsync(Collection, memberId, fields, ct);
    }

    private static Member ToMember(FirestoreDocument doc) => new()
    {
        Id = doc.Id,
        Name = doc.GetString("name"),
        Part = Enum.TryParse<PartType>(doc.GetString("part"), out var part) ? part : PartType.Soprano,
        UpdatedAt = doc.GetDateTime("updatedAt")
    };
}
