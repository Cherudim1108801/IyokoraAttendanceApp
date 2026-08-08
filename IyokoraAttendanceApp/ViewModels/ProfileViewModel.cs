using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly MemberService _memberService;
    private readonly LocalProfileStore _profile;

    public List<PartOption> PartOptions { get; } = PartOption.All;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private PartOption selectedPart;

    [ObservableProperty]
    private string? savedMessage;

    public ProfileViewModel(MemberService memberService, LocalProfileStore profile)
    {
        _memberService = memberService;
        _profile = profile;
        selectedPart = PartOptions[0];
        Load();
    }

    private void Load()
    {
        Name = _profile.Name;
        SelectedPart = PartOptions.First(p => p.Part == _profile.Part);
    }

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
            _profile.Name = trimmedName;
            _profile.Part = SelectedPart.Part;
            await _memberService.SaveAsync(_profile.MemberId, trimmedName, SelectedPart.Part);
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

        _profile.Clear();
        await Shell.Current!.GoToAsync("//onboarding");
    }
}
