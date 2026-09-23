using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;

namespace IyokoraAttendanceApp.ViewModels;

/// <summary>団員管理画面用のViewModel。団員一覧の取得と削除を担う。管理者のみ操作できる。</summary>
public partial class MembersViewModel(MemberService memberService, LocalProfileStore profile) : BaseViewModel
{
    private bool _isLoading;

    public ObservableCollection<Member> Members { get; } = [];

    /// <summary>操作中の利用者が管理者かどうか。団員の削除の可否に使用する。</summary>
    public bool IsAdmin => profile.Role == Role.Admin;

    [RelayCommand]
    public async Task LoadAsync()
    {
        // RefreshView は IsRefreshing (= IsBusy) が true になると、発生源を問わず
        // 自動で Command (LoadCommand) を実行するため、多重実行を防ぐ。
        if (_isLoading)
            return;

        _isLoading = true;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var members = await memberService.GetAllAsync();
            Members.Clear();
            foreach (var member in members)
                Members.Add(member);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"読み込みに失敗しました。({ex.Message})";
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMemberAsync(Member member)
    {
        if (!IsAdmin)
            return;

        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage is null)
            return;

        var confirmed = await currentPage.DisplayAlertAsync(
            "団員を削除",
            $"{member.Name} を削除します。よろしいですか？",
            "はい", "キャンセル");

        if (!confirmed)
            return;

        try
        {
            await memberService.DeleteAsync(member.Id);
            Members.Remove(member);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"削除に失敗しました。({ex.Message})";
        }
    }
}
