using CommunityToolkit.Mvvm.ComponentModel;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>プロフィール画面における、1曲分の内部パート（分割）担当の選択状態。</summary>
public partial class PiecePartSelectionInput : ObservableObject
{
    public required string PieceId { get; init; }
    public required string PieceTitle { get; init; }

    /// <summary>選択可能な内部パート名（例：ソプラノ上／ソプラノ下）。</summary>
    public required List<string> SubPartOptions { get; init; }

    /// <summary>選択中の内部パート名。未選択の場合は null。</summary>
    [ObservableProperty]
    public partial string? SelectedSubPart { get; set; }
}
