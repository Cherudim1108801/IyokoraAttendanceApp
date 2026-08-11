using CommunityToolkit.Mvvm.ComponentModel;
using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>曲登録画面における、1パート分の割り振り入力状態。</summary>
public partial class PartAssignmentInput : ObservableObject
{
    public required PartType Part { get; init; }
    public required string Label { get; init; }

    /// <summary>この曲でパートを使用するかどうか。</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>選択中の内部分割方式。</summary>
    [ObservableProperty]
    public partial PartDivisionOption SelectedDivision { get; set; } = PartDivisionOption.All[0];
}
