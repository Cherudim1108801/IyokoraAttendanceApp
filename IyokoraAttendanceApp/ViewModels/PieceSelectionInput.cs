using CommunityToolkit.Mvvm.ComponentModel;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>練習予定登録画面における、1曲分の選択状態。</summary>
public partial class PieceSelectionInput : ObservableObject
{
    public required string PieceId { get; init; }
    public required string Title { get; init; }

    /// <summary>その日に演奏する曲として選択されているかどうか。</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
