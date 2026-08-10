using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>練習詳細画面用のViewModel。指定練習のパート別出欠一覧を表示する。</summary>
[QueryProperty(nameof(PracticeId), "practiceId")]
public partial class PracticeDetailViewModel(PracticeService practiceService, MemberService memberService, AttendanceService attendanceService, LocalProfileStore profile) : BaseViewModel
{
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

    partial void OnPracticeIdChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
            _ = LoadAsync();
    }

    /// <summary>対象練習の情報と、パート別の出欠一覧を読み込む。</summary>
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
            Practice = await practiceService.GetByIdAsync(PracticeId);
            var members = await memberService.GetAllAsync();
            var attendances = await attendanceService.GetForPracticeAsync(PracticeId);
            var statusByMemberId = attendances.ToDictionary(a => a.MemberId, a => a.Status);

            MyStatus = statusByMemberId.GetValueOrDefault(profile.MemberId, AttendanceStatus.Undecided);
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
            await attendanceService.SetStatusAsync(Practice.Id, profile.MemberId, profile.Name, profile.Part, status);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"更新に失敗しました。({ex.Message})";
        }
    }
}
