using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

public partial class OnboardingViewModel : BaseViewModel
{
    private readonly MemberService _memberService;
    private readonly LocalProfileStore _profile;

    public List<PartOption> PartOptions { get; } = PartOption.All;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private PartOption selectedPart;

    public OnboardingViewModel(MemberService memberService, LocalProfileStore profile)
    {
        _memberService = memberService;
        _profile = profile;
        selectedPart = PartOptions[0];

        if (_profile.IsRegistered)
        {
            name = _profile.Name;
            selectedPart = PartOptions.First(p => p.Part == _profile.Part);
        }
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
        try
        {
            _profile.Name = trimmedName;
            _profile.Part = SelectedPart.Part;
            await _memberService.SaveAsync(_profile.MemberId, trimmedName, SelectedPart.Part);

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
