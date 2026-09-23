using CommunityToolkit.Mvvm.ComponentModel;

namespace IyokoraAttendanceApp.Models;

/// <summary>「音源」タブにおける、練習日ごとにまとめた録音一覧の折りたたみ表示状態。</summary>
public partial class RecordingDateGroup : ObservableObject
{
    public required DateTime Date { get; init; }
    public required string Label { get; init; }
    public required List<RecordingItem> Items { get; init; }

    /// <summary>このグループを展開表示しているかどうか。</summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
}
