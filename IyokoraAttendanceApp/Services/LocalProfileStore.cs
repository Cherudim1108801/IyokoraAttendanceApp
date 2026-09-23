using System.Text.Json;
using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

/// <summary>
/// この端末を使っている本人のプロフィール（ログインなしの簡易識別）を端末内に保存する。
/// </summary>
public class LocalProfileStore
{
    private const string KeyMemberId = "profile.memberId";
    private const string KeyName = "profile.name";
    private const string KeyPart = "profile.part";
    private const string KeyRole = "profile.role";
    private const string KeyPieceParts = "profile.pieceParts";
    private const string KeyLoginId = "profile.loginId";

    /// <summary>名前が登録済みかどうか（オンボーディング完了の判定に使用）。</summary>
    public bool IsRegistered => !string.IsNullOrEmpty(MemberId) && !string.IsNullOrEmpty(Name);

    /// <summary>
    /// この端末に割り当てられた MemberId。未発行の場合は初回アクセス時に自動生成して永続化する。
    /// ログインID でログインした場合は、その持ち主の MemberId に上書きされる。
    /// </summary>
    public string MemberId
    {
        get
        {
            var id = Preferences.Default.Get(KeyMemberId, string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                Preferences.Default.Set(KeyMemberId, id);
            }
            return id;
        }
        set => Preferences.Default.Set(KeyMemberId, value);
    }

    /// <summary>
    /// 複数端末から同じアカウントを使うためのログインID。オンボーディング完了時に発行され、
    /// 別端末では <see cref="MemberId"/> 等と併せてこの値をログイン時に受け取って保存する。
    /// </summary>
    public string LoginId
    {
        get => Preferences.Default.Get(KeyLoginId, string.Empty);
        set => Preferences.Default.Set(KeyLoginId, value);
    }

    /// <summary>表示名。</summary>
    public string Name
    {
        get => Preferences.Default.Get(KeyName, string.Empty);
        set => Preferences.Default.Set(KeyName, value);
    }

    /// <summary>所属パート。未設定時は <see cref="PartType.Soprano"/> を既定値として返す。</summary>
    public PartType Part
    {
        get => Enum.TryParse<PartType>(Preferences.Default.Get(KeyPart, string.Empty), out var part)
            ? part
            : PartType.Soprano;
        set => Preferences.Default.Set(KeyPart, value.ToString());
    }

    /// <summary>役割。未設定時は <see cref="Role.GeneralMember"/> を既定値として返す。</summary>
    public Role Role
    {
        get => Enum.TryParse<Role>(Preferences.Default.Get(KeyRole, string.Empty), out var role)
            ? role
            : Role.GeneralMember;
        set => Preferences.Default.Set(KeyRole, value.ToString());
    }

    /// <summary>曲ごとの内部パート（分割）担当。</summary>
    public List<MemberPiecePart> PieceParts
    {
        get
        {
            var json = Preferences.Default.Get(KeyPieceParts, string.Empty);
            if (string.IsNullOrEmpty(json))
                return [];
            return JsonSerializer.Deserialize<List<MemberPiecePart>>(json) ?? [];
        }
        set => Preferences.Default.Set(KeyPieceParts, JsonSerializer.Serialize(value));
    }

    /// <summary>端末に保存されたプロフィール情報（MemberId／名前／パート／曲ごとのパート担当）をすべて削除する。</summary>
    public void Clear()
    {
        Preferences.Default.Remove(KeyMemberId);
        Preferences.Default.Remove(KeyName);
        Preferences.Default.Remove(KeyPart);
        Preferences.Default.Remove(KeyRole);
        Preferences.Default.Remove(KeyPieceParts);
        Preferences.Default.Remove(KeyLoginId);
    }
}
