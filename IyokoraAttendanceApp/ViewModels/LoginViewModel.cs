using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>IDログイン画面用のViewModel。他の端末で発行されたログインIDから、このアカウントのプロフィールを引き継ぐ。</summary>
public partial class LoginViewModel(MemberService memberService, LocalProfileStore profile) : BaseViewModel
{
    public string LoginIdPlaceholder => $"例：{FirebaseOptions.LoginIdPrefix}1234";

    /// <summary>入力中のログインID。</summary>
    [ObservableProperty]
    public partial string LoginIdInput { get; set; } = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        var normalized = LoginIdGenerator.Normalize(LoginIdInput);
        if (!LoginIdGenerator.IsValidFormat(normalized))
        {
            ErrorMessage = $"IDの形式が正しくありません。（例：{FirebaseOptions.LoginIdPrefix}1234）";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var member = await memberService.FindByLoginIdAsync(normalized);
            if (member is null)
            {
                ErrorMessage = "該当するIDが見つかりませんでした。入力内容をご確認ください。";
                return;
            }

            profile.MemberId = member.Id;
            profile.Name = member.Name;
            profile.Part = member.Part;
            profile.Role = member.Role;
            profile.PieceParts = member.PieceParts;
            profile.LoginId = member.LoginId;

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"ログインに失敗しました。ネットワーク接続と Firebase 設定を確認してください。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task GoToOnboardingAsync()
    {
        if (Shell.Current is not null)
            await Shell.Current.GoToAsync("onboarding");
    }
}
