namespace IyokoraAttendanceApp.Models;

/// <summary>次回練習における、パートごとの参加状況の集計結果（トップ画面表示用）。</summary>
public class PartSummary
{
    public required PartType Part { get; init; }
    public required string Label { get; init; }
    public required Color Color { get; init; }
    public int AttendingCount { get; init; }
    public int MemberCount { get; init; }

    /// <summary>4パート中の最大出席数に対する比率（0〜1）。ProgressBar 表示用。</summary>
    public double BarRatio { get; init; }

    public string CountLabel => $"{AttendingCount} / {MemberCount} 人";
}
