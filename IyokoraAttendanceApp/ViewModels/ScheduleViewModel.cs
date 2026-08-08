using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

public partial class ScheduleViewModel : BaseViewModel
{
    private readonly PracticeService _practiceService;

    public ObservableCollection<Practice> Practices { get; } = [];

    [ObservableProperty]
    private bool isAddPanelVisible;

    [ObservableProperty]
    private DateTime newDate = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private string newTitle = string.Empty;

    [ObservableProperty]
    private string newPlace = string.Empty;

    public ScheduleViewModel(PracticeService practiceService)
    {
        _practiceService = practiceService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var practices = await _practiceService.GetAllAsync();
            Practices.Clear();
            foreach (var practice in practices)
                Practices.Add(practice);
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

    [RelayCommand]
    private void ToggleAddPanel() => IsAddPanelVisible = !IsAddPanelVisible;

    [RelayCommand]
    private async Task AddPracticeAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await _practiceService.CreateAsync(NewDate, NewTitle.Trim(), NewPlace.Trim());
            NewTitle = string.Empty;
            NewPlace = string.Empty;
            NewDate = DateTime.Today.AddDays(7);
            IsAddPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"予定の追加に失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeletePracticeAsync(Practice practice)
    {
        IsBusy = true;
        try
        {
            await _practiceService.DeleteAsync(practice.Id);
            Practices.Remove(practice);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"削除に失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task OpenDetailAsync(Practice practice)
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync($"practiceDetail?practiceId={practice.Id}");
    }
}
