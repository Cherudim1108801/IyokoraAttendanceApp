using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>練習曲（レパートリー）一覧画面用のViewModel。一覧の取得・追加・削除を担う。</summary>
public partial class PiecesViewModel(PieceService pieceService) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<Piece> Pieces { get; } = [];

    /// <summary>曲登録フォームにおける、基本4パートそれぞれの割り振り入力状態。</summary>
    public ObservableCollection<PartAssignmentInput> PartInputs { get; } =
        [.. PartTypeExtensions.All.Select(p => new PartAssignmentInput { Part = p, Label = p.ToDisplayName() })];

    public List<PartDivisionOption> DivisionOptions { get; } = PartDivisionOption.All;

    [ObservableProperty]
    private bool isAddPanelVisible;

    [ObservableProperty]
    private string newTitle = string.Empty;

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
            var pieces = await pieceService.GetAllAsync();
            Pieces.Clear();
            foreach (var piece in pieces)
                Pieces.Add(piece);
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
    private async Task AddPieceAsync()
    {
        var trimmedTitle = NewTitle.Trim();
        if (string.IsNullOrEmpty(trimmedTitle))
        {
            ErrorMessage = "曲名を入力してください。";
            return;
        }

        ErrorMessage = null;
        try
        {
            var assignments = PartInputs
                .Where(p => p.IsSelected)
                .Select(p => new PiecePartAssignment { Part = p.Part, Division = p.SelectedDivision.Division })
                .ToList();

            await pieceService.CreateAsync(trimmedTitle, assignments);

            NewTitle = string.Empty;
            foreach (var input in PartInputs)
            {
                input.IsSelected = false;
                input.SelectedDivision = PartDivisionOption.All[0];
            }
            IsAddPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"曲の追加に失敗しました。({ex.Message})";
        }
    }

    [RelayCommand]
    private async Task DeletePieceAsync(Piece piece)
    {
        try
        {
            await pieceService.DeleteAsync(piece.Id);
            Pieces.Remove(piece);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"削除に失敗しました。({ex.Message})";
        }
    }
}
