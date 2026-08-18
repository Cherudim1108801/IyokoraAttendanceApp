using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>練習詳細画面用のViewModel。指定練習のパート別参加人数と、曲ごとの参加者内訳を表示する。</summary>
[QueryProperty(nameof(PracticeId), "practiceId")]
public partial class PracticeDetailViewModel(
    PracticeService practiceService,
    MemberService memberService,
    AttendanceService attendanceService,
    PieceService pieceService,
    LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<PartSummary> PartSummaries { get; } = [];

    public ObservableCollection<SongParticipation> SongParticipations { get; } = [];

    /// <summary>操作中の利用者が管理者かどうか。録音音源リンクの追加・編集・削除の可否に使用する。</summary>
    public bool IsAdmin => profile.Role == Role.Admin;

    /// <summary>表示対象の練習予定ID（ナビゲーションのクエリパラメータ <c>practiceId</c> から設定される）。</summary>
    [ObservableProperty]
    public partial string PracticeId { get; set; } = string.Empty;

    /// <summary>表示対象の練習予定。未読み込みの場合は null。</summary>
    [ObservableProperty]
    public partial Practice? Practice { get; set; }

    /// <summary>この練習に対する自分の出欠状態。</summary>
    [ObservableProperty]
    public partial AttendanceStatus MyStatus { get; set; } = AttendanceStatus.Undecided;

    /// <summary>この練習の合計参加予定人数。</summary>
    [ObservableProperty]
    public partial int TotalAttending { get; set; }

    /// <summary>登録メンバーの総数。</summary>
    [ObservableProperty]
    public partial int TotalMembers { get; set; }

    /// <summary>演奏予定曲ごとの参加者内訳が1件以上あるかどうか。</summary>
    [ObservableProperty]
    public partial bool HasSongParticipations { get; set; }

    /// <summary>自分の出欠を変更できるかどうか。過去の練習（今日より前）は読み取り専用とする。</summary>
    [ObservableProperty]
    public partial bool IsAttendanceEditable { get; set; } = true;

    /// <summary>パート詳細モーダルで表示中のパート集計。表示していない場合は null。</summary>
    [ObservableProperty]
    public partial PartSummary? SelectedPartSummary { get; set; }

    /// <summary>パート詳細モーダルを表示中かどうか。</summary>
    [ObservableProperty]
    public partial bool IsPartModalVisible { get; set; }

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

    /// <summary>対象練習の情報、パート別参加人数、曲ごとの参加者内訳を読み込む。</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(PracticeId))
            return;

        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行する。SetMyStatusAsync からの呼び出しと
        // 重なると多重実行され、各コレクションに重複した項目が追加されてしまうため、
        // 多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            Practice = await practiceService.GetByIdAsync(PracticeId);
            IsAttendanceEditable = Practice is null || Practice.Date.Date >= DateTime.Today;
            var members = await memberService.GetAllAsync();
            var pieces = await pieceService.GetAllAsync();
            var attendances = await attendanceService.GetForPracticeAsync(PracticeId);
            var statusByMemberId = attendances.ToDictionary(a => a.MemberId, a => a.Status);

            MyStatus = statusByMemberId.GetValueOrDefault(profile.MemberId, AttendanceStatus.Undecided);
            TotalMembers = members.Count;
            TotalAttending = attendances.Count(a => a.Status == AttendanceStatus.Attending);

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

            var membersByPart = members.GroupBy(m => m.Part).ToDictionary(g => g.Key, g => g.Count());

            PartSummaries.Clear();
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

            var attendingMembers = members
                .Where(m => statusByMemberId.GetValueOrDefault(m.Id) == AttendanceStatus.Attending)
                .OrderBy(m => m.Part)
                .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var piecesById = pieces.ToDictionary(p => p.Id);

            SongParticipations.Clear();
            foreach (var pieceRef in Practice?.Pieces ?? [])
            {
                piecesById.TryGetValue(pieceRef.PieceId, out var piece);

                var dots = new List<ParticipationDot>();
                PartType? previousPart = null;
                foreach (var member in attendingMembers)
                {
                    var assignment = piece?.PartAssignments.FirstOrDefault(a => a.Part == member.Part);
                    string? subLabel = null;
                    if (assignment is { Division: PartDivision.UpperLower })
                    {
                        var subPart = member.PieceParts.FirstOrDefault(p => p.PieceId == pieceRef.PieceId)?.SubPart;
                        var labels = assignment.SubPartLabels;
                        if (subPart == labels[0])
                            subLabel = "上";
                        else if (subPart == labels[1])
                            subLabel = "下";
                    }

                    dots.Add(new ParticipationDot
                    {
                        Color = member.Part.ToCardBackgroundColor(),
                        SubLabel = subLabel,
                        Margin = previousPart is not null && previousPart != member.Part
                            ? new Thickness(14, 4, 4, 4)
                            : new Thickness(4)
                    });
                    previousPart = member.Part;
                }

                SongParticipations.Add(new SongParticipation
                {
                    PieceId = pieceRef.PieceId,
                    Title = pieceRef.Title,
                    Dots = dots,
                    RecordingUrl = pieceRef.RecordingUrl,
                    IsFeatured = pieceRef.IsFeatured
                });
            }

            HasSongParticipations = SongParticipations.Count > 0;
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
        if (Practice is null || !IsAttendanceEditable)
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

    [RelayCommand]
    private void ShowPartDetail(PartSummary summary)
    {
        SelectedPartSummary = summary;
        IsPartModalVisible = true;
    }

    [RelayCommand]
    private void ClosePartModal() => IsPartModalVisible = false;

    [RelayCommand]
    private async Task OpenRecordingAsync(SongParticipation song)
    {
        if (!song.HasRecordingUrl)
            return;

        try
        {
            await Launcher.Default.OpenAsync(song.RecordingUrl!);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"リンクを開けませんでした。({ex.Message})";
        }
    }

    [RelayCommand]
    private async Task EditRecordingAsync(SongParticipation song)
    {
        if (!IsAdmin || Practice is null)
            return;

        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is null)
            return;

        var input = await currentPage.DisplayPromptAsync(
            $"{song.Title} の録音リンク",
            "OneDriveの共有リンクを入力してください。空欄で保存するとリンクを削除します。",
            "保存", "キャンセル",
            initialValue: song.RecordingUrl ?? "",
            keyboard: Keyboard.Url);

        if (input is null)
            return;

        var trimmed = input.Trim();
        if (trimmed.Length > 0 && !Uri.TryCreate(trimmed, UriKind.Absolute, out _))
        {
            ErrorMessage = "リンクの形式が正しくありません。";
            return;
        }

        try
        {
            await practiceService.SetPieceRecordingUrlAsync(Practice.Id, song.PieceId, trimmed.Length > 0 ? trimmed : null);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"録音リンクの保存に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private async Task ToggleRecordingFeaturedAsync(SongParticipation song)
    {
        if (!IsAdmin || Practice is null || !song.HasRecordingUrl)
            return;

        try
        {
            await practiceService.SetPieceRecordingFeaturedAsync(Practice.Id, song.PieceId, !song.IsFeatured);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"更新に失敗しました。({ex.Message})";
        }
    }
}
