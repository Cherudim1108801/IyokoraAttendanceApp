using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>ホーム画面（トップページ）用のViewModel。次回練習のパート別参加予定人数を集計する。</summary>
public partial class DashboardViewModel(MemberService memberService, PracticeService practiceService, AttendanceService attendanceService, LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<PartSummary> PartSummaries { get; } = [];

    /// <summary>今日以降で最も近い練習予定。存在しない場合は null。</summary>
    [ObservableProperty]
    public partial Practice? NextPractice { get; set; }

    /// <summary>次回練習予定が登録されているかどうか。</summary>
    [ObservableProperty]
    public partial bool HasNextPractice { get; set; }

    /// <summary>次回練習に対する自分の出欠状態。</summary>
    [ObservableProperty]
    public partial AttendanceStatus MyStatus { get; set; } = AttendanceStatus.Undecided;

    /// <summary>次回練習の合計参加予定人数。</summary>
    [ObservableProperty]
    public partial int TotalAttending { get; set; }

    /// <summary>登録メンバーの総数。</summary>
    [ObservableProperty]
    public partial int TotalMembers { get; set; }

    /// <summary>次回練習について出欠を回答済みのメンバー数（未定を除く）。</summary>
    [ObservableProperty]
    public partial int TotalResponded { get; set; }

    /// <summary>パート詳細モーダルで表示中のパート集計。表示していない場合は null。</summary>
    [ObservableProperty]
    public partial PartSummary? SelectedPartSummary { get; set; }

    /// <summary>パート詳細モーダルを表示中かどうか。</summary>
    [ObservableProperty]
    public partial bool IsPartModalVisible { get; set; }

    public string MyName => profile.Name;

    public string ResponseSummaryLabel => $"回答済み: {TotalResponded} 人 (登録メンバー全 {TotalMembers} 人)";

    partial void OnTotalRespondedChanged(int value) => OnPropertyChanged(nameof(ResponseSummaryLabel));
    partial void OnTotalMembersChanged(int value) => OnPropertyChanged(nameof(ResponseSummaryLabel));

    public bool IsAttendingSelected => MyStatus == AttendanceStatus.Attending;
    public bool IsNotAttendingSelected => MyStatus == AttendanceStatus.NotAttending;
    public bool IsUndecidedSelected => MyStatus == AttendanceStatus.Undecided;

    partial void OnMyStatusChanged(AttendanceStatus value)
    {
        OnPropertyChanged(nameof(IsAttendingSelected));
        OnPropertyChanged(nameof(IsNotAttendingSelected));
        OnPropertyChanged(nameof(IsUndecidedSelected));
    }

    /// <summary>次回練習と、パート別の参加予定人数・参加者名一覧を読み込む。</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行する。OnAppearing での明示呼び出しや
        // SetMyStatusAsync からの呼び出しと重なると多重実行され、PartSummaries に
        // 重複した項目が追加されてしまうため、多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var practice = await practiceService.GetNextUpcomingAsync();
            var members = await memberService.GetAllAsync();

            NextPractice = practice;
            HasNextPractice = practice is not null;
            TotalMembers = members.Count;

            PartSummaries.Clear();

            if (practice is null)
            {
                TotalAttending = 0;
                TotalResponded = 0;
                MyStatus = AttendanceStatus.Undecided;
                return;
            }

            var attendances = await attendanceService.GetForPracticeAsync(practice.Id);
            var mine = attendances.FirstOrDefault(a => a.MemberId == profile.MemberId);
            MyStatus = mine?.Status ?? AttendanceStatus.Undecided;

            var attendingByPart = attendances
                .Where(a => a.Status == AttendanceStatus.Attending)
                .GroupBy(a => a.Part)
                .ToDictionary(g => g.Key, g => g.Count());

            var attendingNamesByPart = attendances
                .Where(a => a.Status == AttendanceStatus.Attending)
                .GroupBy(a => a.Part)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(a => a.MemberName).OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToList());

            var membersByPart = members
                .GroupBy(m => m.Part)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var part in PartTypeExtensions.All)
            {
                PartSummaries.Add(new PartSummary
                {
                    Part = part,
                    Label = part.ToDisplayName(),
                    Color = part.ToColor(),
                    CardBackgroundColor = part.ToCardBackgroundColor(),
                    AttendingCount = attendingByPart.GetValueOrDefault(part),
                    MemberCount = membersByPart.GetValueOrDefault(part),
                    AttendeeNames = attendingNamesByPart.GetValueOrDefault(part) ?? []
                });
            }

            TotalAttending = attendingByPart.Values.Sum();
            TotalResponded = attendances.Count(a => a.Status != AttendanceStatus.Undecided);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"読み込みに失敗しました。ネットワーク接続と Firebase 設定を確認してください。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetMyStatusAsync(AttendanceStatus status)
    {
        if (NextPractice is null)
            return;

        try
        {
            await attendanceService.SetStatusAsync(NextPractice.Id, profile.MemberId, profile.Name, profile.Part, status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"更新に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private static async Task GoToScheduleAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("//schedule");
    }

    [RelayCommand]
    private void ShowPartDetail(PartSummary summary)
    {
        SelectedPartSummary = summary;
        IsPartModalVisible = true;
    }

    [RelayCommand]
    private void ClosePartModal() => IsPartModalVisible = false;

    public void RefreshProfileDisplay() => OnPropertyChanged(nameof(MyName));
}
