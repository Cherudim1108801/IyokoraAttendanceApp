namespace IyokoraAttendanceApp.Models;

/// <summary>1曲に対して複数登録できる、名前付きの録音音源1件分。</summary>
public class PracticeRecording
{
    public required string Id { get; init; }

    /// <summary>録音の名前（任意）。同一曲の複数録音を区別するために使う。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>録音音源へのリンク（OneDriveなど）。</summary>
    public required string Url { get; init; }

    /// <summary>「音源」タブで強調表示（ピン留め）するかどうか。</summary>
    public bool IsFeatured { get; init; }

    /// <summary>一覧表示用の名称。名前が未設定の場合は既定のラベルを返す。</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "録音" : Name;
}
