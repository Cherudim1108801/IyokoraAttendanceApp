namespace IyokoraAttendanceApp.Models;

public class Attendance
{
    public required string Id { get; set; }
    public required string PracticeId { get; set; }
    public required string MemberId { get; set; }
    public required string MemberName { get; set; }
    public required PartType Part { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static string BuildId(string practiceId, string memberId) => $"{practiceId}_{memberId}";
}
