using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>「音源」タブ用のViewModel。録音音源が登録された曲を横断的に一覧表示し、強調表示された録音は上部にまとめる。</summary>
public partial class RecordingsViewModel(PracticeService practiceService) : BaseViewModel
{
    private bool _isLoading;

    /// <summary>強調表示（ピン留め）された録音の一覧。</summary>
    public ObservableCollection<RecordingItem> FeaturedRecordings { get; } = [];

    /// <summary>録音登録済みの曲すべての一覧（練習日の新しい順）。</summary>
    public ObservableCollection<RecordingItem> AllRecordings { get; } = [];

    /// <summary>強調表示された録音が1件以上あるかどうか。</summary>
    [ObservableProperty]
    public partial bool HasFeaturedRecordings { get; set; }

    /// <summary>録音が1件も登録されていないかどうか（空表示の判定に使用）。</summary>
    [ObservableProperty]
    public partial bool HasNoRecordings { get; set; }

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
            var practices = await practiceService.GetAllAsync();

            var items = practices
                .OrderByDescending(p => p.Date)
                .SelectMany(p => p.Pieces
                    .Where(piece => !string.IsNullOrEmpty(piece.RecordingUrl))
                    .Select(piece => new RecordingItem
                    {
                        PracticeId = p.Id,
                        PieceId = piece.PieceId,
                        Title = piece.Title,
                        PracticeLabel = string.IsNullOrEmpty(p.Title)
                            ? $"{p.Date:yyyy年M月d日}の練習"
                            : $"{p.Date:yyyy年M月d日} {p.Title}",
                        RecordingUrl = piece.RecordingUrl!,
                        IsFeatured = piece.IsFeatured
                    }))
                .ToList();

            FeaturedRecordings.Clear();
            foreach (var item in items.Where(i => i.IsFeatured))
                FeaturedRecordings.Add(item);
            HasFeaturedRecordings = FeaturedRecordings.Count > 0;

            AllRecordings.Clear();
            foreach (var item in items)
                AllRecordings.Add(item);
            HasNoRecordings = AllRecordings.Count == 0;
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
    private async Task OpenRecordingAsync(RecordingItem item)
    {
        try
        {
            await Launcher.Default.OpenAsync(item.RecordingUrl);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"リンクを開けませんでした。({ex.Message})";
        }
    }
}
