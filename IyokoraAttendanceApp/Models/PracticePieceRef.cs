namespace IyokoraAttendanceApp.Models;

/// <summary>練習予定における演奏予定曲への参照。曲名は登録時点のスナップショットとして非正規化保持する。</summary>
public class PracticePieceRef
{
    public required string PieceId { get; init; }
    public required string Title { get; init; }

    /// <summary>この練習でのこの曲の録音音源へのリンク（OneDriveなど）。未登録の場合は null。</summary>
    public string? RecordingUrl { get; init; }
}
