namespace IyokoraAttendanceApp.Models;

public enum AttendanceStatus
{
    Undecided,
    Attending,
    NotAttending
}

public static class AttendanceStatusExtensions
{
    public static string ToDisplayName(this AttendanceStatus status) => status switch
    {
        AttendanceStatus.Attending => "参加",
        AttendanceStatus.NotAttending => "不参加",
        AttendanceStatus.Undecided => "未定",
        _ => status.ToString()
    };

    public static Color ToColor(this AttendanceStatus status) => status switch
    {
        AttendanceStatus.Attending => Color.FromArgb("#3EC1A4"),
        AttendanceStatus.NotAttending => Color.FromArgb("#E0607A"),
        AttendanceStatus.Undecided => Color.FromArgb("#B0B0B8"),
        _ => Colors.Gray
    };
}
