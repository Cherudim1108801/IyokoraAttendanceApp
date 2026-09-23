using IyokoraAttendanceApp.ViewModels;

namespace IyokoraAttendanceApp.Views;

public partial class MembersPage : ContentPage
{
    private readonly MembersViewModel _viewModel;

    public MembersPage(MembersViewModel viewModel)
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
