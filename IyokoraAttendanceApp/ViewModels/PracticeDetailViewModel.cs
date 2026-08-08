using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

[QueryProperty(nameof(PracticeId), "practiceId")]
public partial class PracticeDetailViewModel : BaseViewModel
{
    private readonly PracticeService _practiceService;
    private readonly MemberService _memberService;
    private readonly AttendanceService _attendanceService;
    private readonly LocalProfileStore _profile;
    private bool _isLoading;

    public ObservableCollection<PartGroup> PartGroups { get; } = [];

    [ObservableProperty]
    private string practiceId = string.Empty;

    [ObservableProperty]
    private Practice? practice;

    [ObservableProperty]
    private AttendanceStatus myStatus = AttendanceStatus.Undecided;

    [ObservableProperty]
    private int totalAttending;

    [ObservableProperty]
    private int totalMembers;

    public bool IsAttendingSelected => MyStatus == AttendanceStatus.Attending;
    public bool IsNotAttendingSelected => MyStatus == AttendanceStatus.NotAttending;
    public bool IsUndecidedSelected => MyStatus == AttendanceStatus.Undecided;

    public string AttendanceSummaryLabel => $"参加予定: {TotalAttending} / {TotalMembers} 人";

    partial void OnMyStatusChanged(AttendanceStatus value)
    {
        OnPropertyChanged(nameof(IsAttendingSelected));
        OnPropertyChanged(nameof(IsNotAttendingSelected));
        OnPropertyChanged(nameof(IsUndecidedSelected));
    }

    partial void OnTotalAttendingChanged(int value) => OnPropertyChanged(nameof(AttendanceSummaryLabel));
    partial void OnTotalMembersChanged(int value) => OnPropertyChanged(nameof(AttendanceSummaryLabel));

    public PracticeDetailViewModel(PracticeService practiceService, MemberService memberService, AttendanceService attendanceService, LocalProfileStore profile)
    {
        _practiceService = practiceService;
        _memberService = memberService;
        _attendanceService = attendanceService;
        _profile = profile;
    }

    partial void OnPracticeIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(PracticeId))
            return;

        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行する。SetMyStatusAsync からの呼び出しと
        // 重なると多重実行され、PartGroups に重複した項目が追加されてしまうため、
        // 多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Practice = await _practiceService.GetByIdAsync(PracticeId);
            var members = await _memberService.GetAllAsync();
            var attendances = await _attendanceService.GetForPracticeAsync(PracticeId);
            var statusByMemberId = attendances.ToDictionary(a => a.MemberId, a => a.Status);

            MyStatus = statusByMemberId.GetValueOrDefault(_profile.MemberId, AttendanceStatus.Undecided);
            TotalMembers = members.Count;
            TotalAttending = attendances.Count(a => a.Status == AttendanceStatus.Attending);

            PartGroups.Clear();
            foreach (var part in PartTypeExtensions.All)
            {
                var partMembers = members.Where(m => m.Part == part).ToList();
                var attendees = partMembers
                    .Select(m => new AttendeeItem
                    {
                        MemberId = m.Id,
                        Name = m.Name,
                        Status = statusByMemberId.GetValueOrDefault(m.Id, AttendanceStatus.Undecided)
                    })
                    .OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                PartGroups.Add(new PartGroup
                {
                    Part = part,
                    Label = part.ToDisplayName(),
                    Color = part.ToColor(),
                    AttendingCount = attendees.Count(a => a.Status == AttendanceStatus.Attending),
                    TotalCount = partMembers.Count,
                    Attendees = attendees
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
    private async Task SetMyStatusAsync(AttendanceStatus status)
    {
        if (Practice is null)
            return;

        try
        {
            await _attendanceService.SetStatusAsync(Practice.Id, _profile.MemberId, _profile.Name, _profile.Part, status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"更新に失敗しました。({ex.Message})";
        }
    }
}
