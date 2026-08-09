using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly MemberService _memberService;
    private readonly PracticeService _practiceService;
    private readonly AttendanceService _attendanceService;
    private readonly LocalProfileStore _profile;
    private bool _isLoading;

    public ObservableCollection<PartSummary> PartSummaries { get; } = [];

    [ObservableProperty]
    private Practice? nextPractice;

    [ObservableProperty]
    private bool hasNextPractice;

    [ObservableProperty]
    private AttendanceStatus myStatus = AttendanceStatus.Undecided;

    [ObservableProperty]
    private int totalAttending;

    [ObservableProperty]
    private int totalMembers;

    [ObservableProperty]
    private int totalResponded;

    [ObservableProperty]
    private PartSummary? selectedPartSummary;

    [ObservableProperty]
    private bool isPartModalVisible;

    public string MyName => _profile.Name;

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

    public DashboardViewModel(MemberService memberService, PracticeService practiceService, AttendanceService attendanceService, LocalProfileStore profile)
    {
        _memberService = memberService;
        _practiceService = practiceService;
        _attendanceService = attendanceService;
        _profile = profile;
    }

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
            var practice = await _practiceService.GetNextUpcomingAsync();
            var members = await _memberService.GetAllAsync();

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

            var attendances = await _attendanceService.GetForPracticeAsync(practice.Id);
            var mine = attendances.FirstOrDefault(a => a.MemberId == _profile.MemberId);
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
            await _attendanceService.SetStatusAsync(NextPractice.Id, _profile.MemberId, _profile.Name, _profile.Part, status);
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
