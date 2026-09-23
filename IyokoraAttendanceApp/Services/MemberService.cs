using IyokoraAttendanceApp.Models;
using Microsoft.Extensions.Logging;

namespace IyokoraAttendanceApp.Services;

/// <summary>Firestore の <c>members</c> コレクションに対するメンバー情報の取得・保存を担う。氏名の暗号化・復号を担当する。</summary>
public class MemberService(FirestoreClient client, ILogger<MemberService> logger)
{
    private const string Collection = "members";

    /// <summary>登録されている全メンバーを、パート → 氏名の順で取得する。</summary>
    /// <param name="ct">キャンセルトークン。</param>
    public async Task<List<Member>> GetAllAsync(CancellationToken ct = default)
    {
        var docs = await client.ListDocumentsAsync(Collection, ct);
        var members = new List<Member>();
        foreach (var doc in docs.Where(d => d.GetString("groupId") == FirebaseOptions.GroupId))
            members.Add(await ToMemberAsync(doc, ct));

        return members
            .OrderBy(m => m.Part)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>メンバーの名前・パート・役割・曲ごとの内部パート担当を新規登録または更新する。</summary>
    /// <param name="memberId">端末で発行された MemberId。</param>
    /// <param name="name">表示名。</param>
    /// <param name="part">所属パート。</param>
    /// <param name="role">役割（管理者／一般団員）。</param>
    /// <param name="pieceParts">曲ごとの内部パート（分割）担当。</param>
    /// <param name="ct">キャンセルトークン。</param>
    public Task SaveAsync(string memberId, string name, PartType part, Role role, IReadOnlyList<MemberPiecePart> pieceParts, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, object?>
        {
            ["groupId"] = FirebaseOptions.GroupId,
            ["name"] = NameCipher.Encrypt(name),
            ["part"] = part.ToString(),
            ["role"] = role.ToString(),
            ["pieceParts"] = pieceParts
                .Select(p => new Dictionary<string, object?>
                {
                    ["pieceId"] = p.PieceId,
                    ["subPart"] = p.SubPart
                })
                .Cast<object?>()
                .ToList(),
            ["updatedAt"] = DateTime.UtcNow
        };
        return client.UpsertDocumentAsync(Collection, memberId, fields, ct);
    }

    private async Task<Member> ToMemberAsync(FirestoreDocument doc, CancellationToken ct)
    {
        var decrypted = NameCipher.DecryptOrPlain(doc.GetString("name"));
        if (decrypted.WasLegacyFormat)
            _ = ReencryptNameInBackgroundAsync(doc.Id, decrypted.Value, ct);

        return new Member
        {
            Id = doc.Id,
            Name = decrypted.Value,
            Part = Enum.TryParse<PartType>(doc.GetString("part"), out var part) ? part : PartType.Soprano,
            Role = Enum.TryParse<Role>(doc.GetString("role"), out var role) ? role : Role.GeneralMember,
            PieceParts = doc.GetList("pieceParts")
                .OfType<Dictionary<string, object?>>()
                .Select(ToPiecePart)
                .ToList(),
            UpdatedAt = doc.GetDateTime("updatedAt")
        };
    }

    /// <summary>
    /// 氏名が旧 AES-CBC 形式で暗号化されていた場合に、画面の表示や読み込みをブロックせず
    /// バックグラウンドで AES-GCM に再暗号化して保存し直す。ネットワーク障害等で失敗しても
    /// 画面表示には影響させず、次にこのメンバーの氏名が読み取られた際に再度移行を試みる。
    /// </summary>
    private async Task ReencryptNameInBackgroundAsync(string memberId, string plainName, CancellationToken ct)
    {
        try
        {
            var fields = new Dictionary<string, object?> { ["name"] = NameCipher.Encrypt(plainName) };
            await client.UpsertDocumentAsync(Collection, memberId, fields, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "氏名の暗号化方式の移行(AES-CBC→AES-GCM)に失敗しました: memberId={MemberId}", memberId);
        }
    }

    private static MemberPiecePart ToPiecePart(Dictionary<string, object?> fields) => new()
    {
        PieceId = fields.GetValueOrDefault("pieceId") as string ?? string.Empty,
        SubPart = fields.GetValueOrDefault("subPart") as string ?? string.Empty
    };
}
