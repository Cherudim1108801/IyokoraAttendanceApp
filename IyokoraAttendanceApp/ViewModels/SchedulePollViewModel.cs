using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>日程投票画面用のViewModel。候補日の一覧取得・追加・削除と、各候補日への参加意思の投票を担う。候補日の追加・削除は管理者のみ行える。</summary>
public partial class SchedulePollViewModel(ScheduleCandidateService scheduleCandidateService, ScheduleVoteService scheduleVoteService, LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<ScheduleCandidateDisplay> Candidates { get; } = [];

    /// <summary>操作中の利用者が管理者かどうか。候補日の追加・削除の可否に使用する。</summary>
    public bool IsAdmin => profile.Role == Role.Admin;

    public List<TimeOfDayOption> TimeOfDayOptions { get; } = TimeOfDayOption.All;

    /// <summary>候補日の追加パネルを表示中かどうか。</summary>
    [ObservableProperty]
    public partial bool IsAddPanelVisible { get; set; }

    /// <summary>入力中の候補日。</summary>
    [ObservableProperty]
    public partial DateTime NewDate { get; set; } = DateTime.Today.AddDays(7);

    /// <summary>選択中の時間帯区分。</summary>
    [ObservableProperty]
    public partial TimeOfDayOption NewTimeOfDay { get; set; } = TimeOfDayOption.All[0];

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
            var candidates = await scheduleCandidateService.GetUpcomingAsync();
            var votes = await scheduleVoteService.GetForCandidatesAsync(candidates.Select(c => c.Id));

            var attendingByCandidate = votes
                .Where(v => v.Status == AttendanceStatus.Attending)
                .GroupBy(v => v.CandidateId)
                .ToDictionary(g => g.Key, g => g.Count());

            var notAttendingByCandidate = votes
                .Where(v => v.Status == AttendanceStatus.NotAttending)
                .GroupBy(v => v.CandidateId)
                .ToDictionary(g => g.Key, g => g.Count());

            var myStatusByCandidate = votes
                .Where(v => v.MemberId == profile.MemberId)
                .ToDictionary(v => v.CandidateId, v => v.Status);

            Candidates.Clear();
            foreach (var candidate in candidates)
            {
                Candidates.Add(new ScheduleCandidateDisplay
                {
                    Candidate = candidate,
                    MyStatus = myStatusByCandidate.GetValueOrDefault(candidate.Id, AttendanceStatus.Undecided),
                    AttendingCount = attendingByCandidate.GetValueOrDefault(candidate.Id),
                    NotAttendingCount = notAttendingByCandidate.GetValueOrDefault(candidate.Id)
                });
            }
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
    private async Task AddCandidateAsync()
    {
        if (!IsAdmin)
            return;

        ErrorMessage = null;
        try
        {
            await scheduleCandidateService.CreateAsync(NewDate, NewTimeOfDay.TimeOfDay);
            NewDate = DateTime.Today.AddDays(7);
            NewTimeOfDay = TimeOfDayOption.All[0];
            IsAddPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"候補日の追加に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private async Task DeleteCandidateAsync(ScheduleCandidateDisplay item)
    {
        if (!IsAdmin)
            return;

        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is null)
            return;

        var confirmed = await currentPage.DisplayAlertAsync(
            "候補日を削除",
            "この候補日を削除します。登録済みの投票もすべて削除されます。よろしいですか？",
            "はい", "キャンセル");

        if (!confirmed)
            return;

        try
        {
            await scheduleCandidateService.DeleteAsync(item.Candidate.Id);
            Candidates.Remove(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"削除に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private Task VoteAttendingAsync(ScheduleCandidateDisplay item) => SetMyVoteAsync(item, AttendanceStatus.Attending);

    [RelayCommand]
    private Task VoteNotAttendingAsync(ScheduleCandidateDisplay item) => SetMyVoteAsync(item, AttendanceStatus.NotAttending);

    private async Task SetMyVoteAsync(ScheduleCandidateDisplay item, AttendanceStatus status)
    {
        try
        {
            await scheduleVoteService.SetStatusAsync(item.Candidate.Id, profile.MemberId, profile.Name, status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"投票の更新に失敗しました。({ex.Message})";
        }
    }
}
