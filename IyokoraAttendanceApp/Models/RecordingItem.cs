namespace IyokoraAttendanceApp.Models;

/// <summary>「音源」タブに表示する、1件の録音音源データ。</summary>
public class RecordingItem
{
    public required string PracticeId { get; init; }
    public required string PieceId { get; init; }

    /// <summary>録音ID。</summary>
    public required string RecordingId { get; init; }

    /// <summary>曲名。</summary>
    public required string Title { get; init; }

    /// <summary>録音の名前（任意）。同一曲の複数録音を区別するための表示に使う。</summary>
    public string? RecordingName { get; init; }

    /// <summary>この録音が行われた練習の日付を含む表示用ラベル。</summary>
    public required string PracticeLabel { get; init; }

    /// <summary>録音音源へのリンク(OneDriveなど)。</summary>
    public required string RecordingUrl { get; init; }

    /// <summary>強調表示(ピン留め)されているかどうか。</summary>
    public bool IsFeatured { get; init; }
}
