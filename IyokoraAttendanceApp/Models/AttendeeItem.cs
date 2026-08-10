namespace IyokoraAttendanceApp.Models;

/// <summary>練習詳細画面のメンバー一覧に表示する、1人分の出欠表示用データ。</summary>
public class AttendeeItem
{
    public required string MemberId { get; init; }
    public required string Name { get; init; }
    public AttendanceStatus Status { get; init; }
    public string StatusLabel => Status.ToDisplayName();
    public Color StatusColor => Status.ToColor();
}

/// <summary>練習詳細画面における、パートごとの出欠集計とメンバー一覧（表示用データ）。</summary>
public class PartGroup
{
    public required PartType Part { get; init; }
    public required string Label { get; init; }
    public required Color Color { get; init; }
    public int AttendingCount { get; init; }
    public int TotalCount { get; init; }
    public required List<AttendeeItem> Attendees { get; init; }
}
