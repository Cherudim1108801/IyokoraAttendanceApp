namespace IyokoraAttendanceApp.Models;

public class Practice
{
    public required string Id { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Place { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
