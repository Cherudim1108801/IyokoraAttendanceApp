using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>過去の練習データ一覧画面用のViewModel。今日より前の練習予定を日付の新しい順に表示する。削除は管理者のみ行える。</summary>
public partial class PracticeHistoryViewModel(PracticeService practiceService, LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<Practice> Practices { get; } = [];

    /// <summary>操作中の利用者が管理者かどうか。過去の練習予定の削除可否に使用する。</summary>
    public bool IsAdmin => profile.Role == Role.Admin;

    [RelayCommand]
    public async Task LoadAsync()
    {
        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行するため、多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var practices = await practiceService.GetPastAsync();
            Practices.Clear();
            foreach (var practice in practices)
                Practices.Add(practice);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"読み込みに失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeletePracticeAsync(Practice practice)
    {
        if (!IsAdmin)
            return;

        try
        {
            await practiceService.DeleteAsync(practice.Id);
            Practices.Remove(practice);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"削除に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private static async Task OpenDetailAsync(Practice practice)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"practiceDetail?practiceId={practice.Id}");
    }
}
