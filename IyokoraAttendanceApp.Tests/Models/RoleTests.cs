using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class RoleTests
{
    [Theory]
    [InlineData(Role.GeneralMember, "一般団員")]
    [InlineData(Role.Admin, "管理者")]
    public void ToDisplayName_ReturnsExpectedJapaneseLabel(Role role, string expected)
    {
        Assert.Equal(expected, role.ToDisplayName());
    }

    [Fact]
    public void All_ContainsEveryRoleInGeneralMemberFirstOrder()
    {
        Assert.Equal([Role.GeneralMember, Role.Admin], RoleExtensions.All);
    }
}
