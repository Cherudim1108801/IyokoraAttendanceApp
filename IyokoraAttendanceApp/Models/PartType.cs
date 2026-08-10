namespace IyokoraAttendanceApp.Models;

/// <summary>合唱におけるパート区分。</summary>
public enum PartType
{
    Soprano,
    Alto,
    Tenor,
    Bass
}

/// <summary><see cref="PartType"/> の表示用ヘルパー。</summary>
public static class PartTypeExtensions
{
    /// <summary>UI表示用の日本語名を返す。</summary>
    public static string ToDisplayName(this PartType part) => part switch
    {
        PartType.Soprano => "ソプラノ",
        PartType.Alto => "アルト",
        PartType.Tenor => "テナー",
        PartType.Bass => "ベース",
        _ => part.ToString()
    };

    /// <summary>パートを識別するためのUI表示色を返す。</summary>
    public static Color ToColor(this PartType part) => part switch
    {
        PartType.Soprano => Color.FromArgb("#FF6B9D"),
        PartType.Alto => Color.FromArgb("#7B7FF6"),
        PartType.Tenor => Color.FromArgb("#3EC1A4"),
        PartType.Bass => Color.FromArgb("#4A4A6A"),
        _ => Colors.Gray
    };

    /// <summary>全パートを固定順（ソプラノ→アルト→テナー→ベース）で列挙する。</summary>
    public static readonly PartType[] All =
    [
        PartType.Soprano,
        PartType.Alto,
        PartType.Tenor,
        PartType.Bass
    ];
}
