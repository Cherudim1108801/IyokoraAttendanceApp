using IyokoraAttendanceApp.ViewModels;

namespace IyokoraAttendanceApp.Views;

public partial class PracticeDetailPage : ContentPage
{
    public PracticeDetailPage(PracticeDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
