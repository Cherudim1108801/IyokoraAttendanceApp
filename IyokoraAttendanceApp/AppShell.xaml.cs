using IyokoraAttendanceApp.Services;
using IyokoraAttendanceApp.Views;

namespace IyokoraAttendanceApp
{
    public partial class AppShell : Shell
    {
        private readonly LocalProfileStore _profile;

        public AppShell(LocalProfileStore profile)
        {
            InitializeComponent();
            _profile = profile;

            Routing.RegisterRoute("onboarding", typeof(OnboardingPage));
            Routing.RegisterRoute("practiceDetail", typeof(PracticeDetailPage));

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            if (!_profile.IsRegistered)
            {
                await GoToAsync("onboarding");
            }
        }
    }
}
