using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>プロフィール画面用のViewModel。自分の名前・パートの編集とプロフィール切り替えを担う。</summary>
public partial class ProfileViewModel(MemberService memberService, LocalProfileStore profile) : BaseViewModel
{
    public List<PartOption> PartOptions { get; } = PartOption.All;

    [ObservableProperty]
    private string name = profile.Name;

    [ObservableProperty]
    private PartOption selectedPart = PartOption.All.First(p => p.Part == profile.Part);

    [ObservableProperty]
    private string? savedMessage;

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
        SavedMessage = null;
        try
        {
            profile.Name = trimmedName;
            profile.Part = SelectedPart.Part;
            await memberService.SaveAsync(profile.MemberId, trimmedName, SelectedPart.Part);
            SavedMessage = "保存しました。";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存に失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchProfileAsync()
    {
        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is null)
            return;

        var confirmed = await currentPage.DisplayAlertAsync(
            "別のプロフィールを使う",
            "この端末に保存されている名前・パートの情報を削除し、最初の登録画面に戻ります。よろしいですか？",
            "はい", "キャンセル");

        if (!confirmed)
            return;

        profile.Clear();
        await Shell.Current!.GoToAsync("onboarding");
    }
}
