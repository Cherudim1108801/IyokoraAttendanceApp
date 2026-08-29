namespace IyokoraAttendanceApp.Models;

/// <summary>Picker などの役割選択UIに表示するための、役割と表示名の組。</summary>
public class RoleOption
{
    public required Role Role { get; init; }
    public required string Label { get; init; }

    /// <summary>全役割を選択肢として列挙したもの。</summary>
    public static List<RoleOption> All { get; } = RoleExtensions.All
        .Select(r => new RoleOption { Role = r, Label = r.ToDisplayName() })
        .ToList();
}
