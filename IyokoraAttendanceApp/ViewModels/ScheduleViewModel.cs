using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>練習予定一覧画面用のViewModel。今日以降の練習予定一覧の取得・追加・削除を担う。追加・削除は管理者のみ行える。過去の練習は「履歴」タブで確認する。</summary>
public partial class ScheduleViewModel(PracticeService practiceService, PieceService pieceService, LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<Practice> Practices { get; } = [];

    /// <summary>操作中の利用者が管理者かどうか。練習予定の追加・削除の可否に使用する。</summary>
    public bool IsAdmin => profile.Role == Role.Admin;

    /// <summary>練習予定登録フォームにおける、レパートリー曲の選択状態一覧。</summary>
    public ObservableCollection<PieceSelectionInput> PieceInputs { get; } = [];

    /// <summary>練習予定の追加パネルを表示中かどうか。</summary>
    [ObservableProperty]
    public partial bool IsAddPanelVisible { get; set; }

    /// <summary>入力中の練習日。</summary>
    [ObservableProperty]
    public partial DateTime NewDate { get; set; } = DateTime.Today.AddDays(7);

    /// <summary>入力中のタイトル（任意）。</summary>
    [ObservableProperty]
    public partial string NewTitle { get; set; } = string.Empty;

    /// <summary>入力中の場所（任意）。</summary>
    [ObservableProperty]
    public partial string NewPlace { get; set; } = string.Empty;

    /// <summary>入力中の開始時刻（任意）。</summary>
    [ObservableProperty]
    public partial TimeOnly? NewStartTime { get; set; }

    /// <summary>入力中の終了時刻（任意）。</summary>
    [ObservableProperty]
    public partial TimeOnly? NewEndTime { get; set; }

    /// <summary>入力中のタイムスケジュール項目一覧（任意・10分単位）。</summary>
    public ObservableCollection<TimelineItemInput> NewTimelineItems { get; } = [];

    /// <summary>鍵の受け取りが必要かどうか。管理者のみが参照できる情報として練習予定に保存される。</summary>
    [ObservableProperty]
    public partial bool NewRequiresKeyPickup { get; set; }

    [RelayCommand]
    private void AddTimelineItem() => NewTimelineItems.Add(new TimelineItemInput());

    [RelayCommand]
    private void RemoveTimelineItem(TimelineItemInput item) => NewTimelineItems.Remove(item);

    [RelayCommand]
    public async Task LoadAsync()
    {
        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行する。AddPracticeAsync 等からの呼び出しと
        // 重なると多重実行され、Practices に重複した項目が追加されてしまうため、
        // 多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var practices = await practiceService.GetUpcomingAsync();
            Practices.Clear();
            foreach (var practice in practices)
                Practices.Add(practice);

            var pieces = await pieceService.GetAllAsync();
            PieceInputs.Clear();
            foreach (var piece in pieces)
                PieceInputs.Add(new PieceSelectionInput { PieceId = piece.Id, Title = piece.Title });
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
    private void ToggleAddPanel() => IsAddPanelVisible = !IsAddPanelVisible;

    [RelayCommand]
    private async Task AddPracticeAsync()
    {
        if (!IsAdmin)
            return;

        ErrorMessage = null;

        if (!PracticeScheduleValidator.TryValidateTimeRange(NewStartTime, NewEndTime, out var rangeError))
        {
            ErrorMessage = rangeError;
            return;
        }

        var timelineItems = new List<PracticeTimelineItem>();
        foreach (var item in NewTimelineItems)
        {
            if (!PracticeScheduleValidator.TryValidateTimelineItem(item, out var itemError))
            {
                ErrorMessage = itemError;
                return;
            }
            timelineItems.Add(new PracticeTimelineItem { StartTime = item.StartTime!.Value.ToString("HH:mm"), EndTime = item.EndTime!.Value.ToString("HH:mm"), Content = item.Content.Trim() });
        }

        try
        {
            var selectedPieces = PieceInputs
                .Where(p => p.IsSelected)
                .Select(p => new PracticePieceRef { PieceId = p.PieceId, Title = p.Title })
                .ToList();

            var startTime = NewStartTime?.ToString("HH:mm") ?? string.Empty;
            var endTime = NewEndTime?.ToString("HH:mm") ?? string.Empty;
            await practiceService.CreateAsync(NewDate, NewTitle.Trim(), NewPlace.Trim(), startTime, endTime, timelineItems, selectedPieces, NewRequiresKeyPickup);
            NewTitle = string.Empty;
            NewPlace = string.Empty;
            NewStartTime = null;
            NewEndTime = null;
            NewTimelineItems.Clear();
            NewDate = DateTime.Today.AddDays(7);
            NewRequiresKeyPickup = false;
            IsAddPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"予定の追加に失敗しました。({ex.Message})";
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
