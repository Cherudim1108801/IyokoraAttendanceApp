using IyokoraAttendanceApp.ViewModels;

namespace IyokoraAttendanceApp.Views;

public partial class PracticeHistoryPage : ContentPage
{
    private readonly PracticeHistoryViewModel _viewModel;

    public PracticeHistoryPage(PracticeHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
