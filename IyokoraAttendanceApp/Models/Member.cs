namespace IyokoraAttendanceApp.Models;

public class Member
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required PartType Part { get; set; }
    public DateTime UpdatedAt { get; set; }
}
