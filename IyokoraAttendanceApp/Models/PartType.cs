namespace IyokoraAttendanceApp.Models;

public enum PartType
{
    Soprano,
    Alto,
    Tenor,
    Bass
}

public static class PartTypeExtensions
{
    public static string ToDisplayName(this PartType part) => part switch
    {
        PartType.Soprano => "ソプラノ",
        PartType.Alto => "アルト",
        PartType.Tenor => "テナー",
        PartType.Bass => "ベース",
        _ => part.ToString()
    };

    public static Color ToColor(this PartType part) => part switch
    {
        PartType.Soprano => Color.FromArgb("#FF6B9D"),
        PartType.Alto => Color.FromArgb("#7B7FF6"),
        PartType.Tenor => Color.FromArgb("#3EC1A4"),
        PartType.Bass => Color.FromArgb("#4A4A6A"),
        _ => Colors.Gray
    };

    public static readonly PartType[] All =
    [
        PartType.Soprano,
        PartType.Alto,
        PartType.Tenor,
        PartType.Bass
    ];
}
