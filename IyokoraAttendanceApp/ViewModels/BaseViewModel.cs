using CommunityToolkit.Mvvm.ComponentModel;

namespace IyokoraAttendanceApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    /// <summary>非同期処理の実行中かどうか。</summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>直近の処理で発生したエラーメッセージ。エラーがない場合は null。</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }
}
