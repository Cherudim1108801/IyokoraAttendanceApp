using CommunityToolkit.Mvvm.ComponentModel;
using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>曲登録画面における、1パート分の割り振り入力状態。</summary>
public partial class PartAssignmentInput : ObservableObject
{
    public required PartType Part { get; init; }
    public required string Label { get; init; }

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private PartDivisionOption selectedDivision = PartDivisionOption.All[0];
}
