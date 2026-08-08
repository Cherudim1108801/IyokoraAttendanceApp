using IyokoraAttendanceApp.ViewModels;

namespace IyokoraAttendanceApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshProfileDisplay();
        _viewModel.LoadCommand.Execute(null);
    }
}
