using CommunityToolkit.Mvvm.ComponentModel;
using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>日程投票画面における、候補日1件分の表示状態（候補日情報＋自分の投票状態＋集計）。</summary>
public partial class ScheduleCandidateDisplay : ObservableObject
{
    public required ScheduleCandidate Candidate { get; init; }

    /// <summary>この候補日に対する自分の参加意思。未投票の場合は <see cref="AttendanceStatus.Undecided"/>。</summary>
    [ObservableProperty]
    public partial AttendanceStatus MyStatus { get; set; }

    [ObservableProperty]
    public partial int AttendingCount { get; set; }

    [ObservableProperty]
    public partial int NotAttendingCount { get; set; }

    public bool IsAttendingSelected => MyStatus == AttendanceStatus.Attending;
    public bool IsNotAttendingSelected => MyStatus == AttendanceStatus.NotAttending;

    /// <summary>一覧表示用：候補日の時間帯区分の日本語表示名。</summary>
    public string TimeOfDayLabel => Candidate.TimeOfDay.ToDisplayName();

    partial void OnMyStatusChanged(AttendanceStatus value)
    {
        OnPropertyChanged(nameof(IsAttendingSelected));
        OnPropertyChanged(nameof(IsNotAttendingSelected));
    }
}
