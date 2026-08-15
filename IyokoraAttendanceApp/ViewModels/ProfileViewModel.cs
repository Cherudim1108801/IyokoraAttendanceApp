using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>プロフィール画面用のViewModel。自分の名前・パート・曲ごとの内部パート担当の編集とプロフィール切り替えを担う。</summary>
public partial class ProfileViewModel(MemberService memberService, PieceService pieceService, LocalProfileStore profile) : BaseViewModel
{
    private List<Piece> _pieces = [];

    public List<PartOption> PartOptions { get; } = PartOption.All;

    public List<RoleOption> RoleOptions { get; } = RoleOption.All;

    /// <summary>所属パートで内部分割が設定されている曲の、パート担当選択一覧。</summary>
    public ObservableCollection<PiecePartSelectionInput> PiecePartInputs { get; } = [];

    /// <summary>編集中の表示名。</summary>
    [ObservableProperty]
    public partial string Name { get; set; } = profile.Name;

    /// <summary>選択中のパート。</summary>
    [ObservableProperty]
    public partial PartOption SelectedPart { get; set; } = PartOption.All.First(p => p.Part == profile.Part);

    /// <summary>選択中の役割。</summary>
    [ObservableProperty]
    public partial RoleOption SelectedRole { get; set; } = RoleOption.All.First(r => r.Role == profile.Role);

    /// <summary>保存完了メッセージ。未保存または保存操作前は null。</summary>
    [ObservableProperty]
    public partial string? SavedMessage { get; set; }

    partial void OnSelectedPartChanged(PartOption value) => BuildPiecePartInputs();

    /// <summary>曲一覧を読み込み、所属パートに応じたパート担当選択一覧を構築する。</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            _pieces = await pieceService.GetAllAsync();
            BuildPiecePartInputs();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"読み込みに失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildPiecePartInputs()
    {
        var savedParts = profile.PieceParts.ToDictionary(p => p.PieceId, p => p.SubPart);

        PiecePartInputs.Clear();
        foreach (var piece in _pieces)
        {
            var assignment = piece.PartAssignments.FirstOrDefault(a => a.Part == SelectedPart.Part);
            if (assignment is null || assignment.Division == PartDivision.None)
                continue;

            PiecePartInputs.Add(new PiecePartSelectionInput
            {
                PieceId = piece.Id,
                PieceTitle = piece.Title,
                SubPartOptions = [.. assignment.SubPartLabels],
                SelectedSubPart = savedParts.GetValueOrDefault(piece.Id)
            });
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var trimmedName = Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
        {
            ErrorMessage = "名前を入力してください。";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        SavedMessage = null;
        try
        {
            var pieceParts = PiecePartInputs
                .Where(p => !string.IsNullOrEmpty(p.SelectedSubPart))
                .Select(p => new MemberPiecePart { PieceId = p.PieceId, SubPart = p.SelectedSubPart! })
                .ToList();

            profile.Name = trimmedName;
            profile.Part = SelectedPart.Part;
            profile.Role = SelectedRole.Role;
            profile.PieceParts = pieceParts;
            await memberService.SaveAsync(profile.MemberId, trimmedName, SelectedPart.Part, SelectedRole.Role, pieceParts);
            SavedMessage = "保存しました。";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存に失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchProfileAsync()
    {
        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is null)
            return;

        var confirmed = await currentPage.DisplayAlertAsync(
            "別のプロフィールを使う",
            "この端末に保存されている名前・パートの情報を削除し、最初の登録画面に戻ります。よろしいですか？",
            "はい", "キャンセル");

        if (!confirmed)
            return;

        profile.Clear();
        await Shell.Current!.GoToAsync("onboarding");
    }
}
