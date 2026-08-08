using CommunityToolkit.Mvvm.ComponentModel;

namespace IyokoraAttendanceApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;
}
