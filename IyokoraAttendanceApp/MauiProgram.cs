using IyokoraAttendanceApp.Services;
using IyokoraAttendanceApp.ViewModels;
using IyokoraAttendanceApp.Views;
using Microsoft.Extensions.Logging;

namespace IyokoraAttendanceApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<FirestoreClient>();
            builder.Services.AddSingleton<LocalProfileStore>();
            builder.Services.AddSingleton<MemberService>();
            builder.Services.AddSingleton<PracticeService>();
            builder.Services.AddSingleton<AttendanceService>();
            builder.Services.AddSingleton<PieceService>();

            // ViewModels
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ScheduleViewModel>();
            builder.Services.AddTransient<PracticeHistoryViewModel>();
            builder.Services.AddTransient<RecordingsViewModel>();
            builder.Services.AddTransient<PracticeDetailViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddTransient<PiecesViewModel>();

            // Views
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<SchedulePage>();
            builder.Services.AddTransient<PracticeHistoryPage>();
            builder.Services.AddTransient<RecordingsPage>();
            builder.Services.AddTransient<PracticeDetailPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<PiecesPage>();

            // Shell
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
