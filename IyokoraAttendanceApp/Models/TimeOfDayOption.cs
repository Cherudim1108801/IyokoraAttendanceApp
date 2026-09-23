namespace IyokoraAttendanceApp.Models;

/// <summary>Picker などの時間帯選択UIに表示するための、時間帯区分と表示名の組。</summary>
public class TimeOfDayOption
{
    public required TimeOfDay TimeOfDay { get; init; }
    public required string Label { get; init; }

    /// <summary>全時間帯区分を選択肢として列挙したもの。</summary>
    public static List<TimeOfDayOption> All { get; } = TimeOfDayExtensions.All
        .Select(t => new TimeOfDayOption { TimeOfDay = t, Label = t.ToDisplayName() })
        .ToList();
}
