using IyokoraAttendanceApp.ViewModels;

namespace IyokoraAttendanceApp.Views;

public partial class PiecesPage : ContentPage
{
    private readonly PiecesViewModel _viewModel;

    public PiecesPage(PiecesViewModel viewModel)
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
