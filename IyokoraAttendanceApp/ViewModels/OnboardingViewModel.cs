using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>初回登録（オンボーディング）画面用のViewModel。名前・パートの登録を担う。</summary>
public partial class OnboardingViewModel(MemberService memberService, LocalProfileStore profile) : BaseViewModel
{
    public List<PartOption> PartOptions { get; } = PartOption.All;

    /// <summary>入力中の表示名。</summary>
    [ObservableProperty]
    public partial string Name { get; set; } = profile.IsRegistered ? profile.Name : string.Empty;

    /// <summary>選択中のパート。</summary>
    [ObservableProperty]
    public partial PartOption SelectedPart { get; set; } = profile.IsRegistered
        ? PartOption.All.First(p => p.Part == profile.Part)
        : PartOption.All[0];

    [RelayCommand]
    private async Task SaveAsync()
    {
        var trimmedName = Name.Trim();
        if (string.IsNullOrEmpty(trimmedName))
        {
            ErrorMessage = "名前を入力してください。";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            profile.Name = trimmedName;
            profile.Part = SelectedPart.Part;
            await memberService.SaveAsync(profile.MemberId, trimmedName, SelectedPart.Part, profile.Role, []);

            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存に失敗しました。ネットワーク接続と Firebase 設定を確認してください。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
