namespace IyokoraAttendanceApp.Models;

public class PartOption
{
    public required PartType Part { get; init; }
    public required string Label { get; init; }

    public static List<PartOption> All { get; } = PartTypeExtensions.All
        .Select(p => new PartOption { Part = p, Label = p.ToDisplayName() })
        .ToList();
}
