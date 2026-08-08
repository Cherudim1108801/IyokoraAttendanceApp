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

    public bool IsRegistered => !string.IsNullOrEmpty(MemberId) && !string.IsNullOrEmpty(Name);

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
    }

    public string Name
    {
        get => Preferences.Default.Get(KeyName, string.Empty);
        set => Preferences.Default.Set(KeyName, value);
    }

    public PartType Part
    {
        get => Enum.TryParse<PartType>(Preferences.Default.Get(KeyPart, string.Empty), out var part)
            ? part
            : PartType.Soprano;
        set => Preferences.Default.Set(KeyPart, value.ToString());
    }

    public void Clear()
    {
        Preferences.Default.Remove(KeyMemberId);
        Preferences.Default.Remove(KeyName);
        Preferences.Default.Remove(KeyPart);
    }
}
