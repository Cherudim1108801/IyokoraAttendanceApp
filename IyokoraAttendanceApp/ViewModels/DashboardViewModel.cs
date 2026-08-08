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

            var membersByPart = members
                .GroupBy(m => m.Part)
                .ToDictionary(g => g.Key, g => g.Count());

            var maxCount = Math.Max(1, PartTypeExtensions.All.Select(p => attendingByPart.GetValueOrDefault(p)).DefaultIfEmpty(0).Max());

            foreach (var part in PartTypeExtensions.All)
            {
                var attendingCount = attendingByPart.GetValueOrDefault(part);
                PartSummaries.Add(new PartSummary
                {
                    Part = part,
                    Label = part.ToDisplayName(),
                    Color = part.ToColor(),
                    AttendingCount = attendingCount,
                    MemberCount = membersByPart.GetValueOrDefault(part),
                    BarRatio = (double)attendingCount / maxCount
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
        }
    }

    [RelayCommand]
    private async Task SetMyStatusAsync(AttendanceStatus status)
    {
        if (NextPractice is null)
            return;

        IsBusy = true;
        try
        {
            await _attendanceService.SetStatusAsync(NextPractice.Id, _profile.MemberId, _profile.Name, _profile.Part, status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"更新に失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task GoToScheduleAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("//schedule");
    }

    public void RefreshProfileDisplay() => OnPropertyChanged(nameof(MyName));
}
