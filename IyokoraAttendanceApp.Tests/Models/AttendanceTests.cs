using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class AttendanceTests
{
    [Fact]
    public void BuildId_CombinesPracticeIdAndMemberIdWithUnderscore()
    {
        var id = Attendance.BuildId("practice-1", "member-2");

        Assert.Equal("practice-1_member-2", id);
    }
}
