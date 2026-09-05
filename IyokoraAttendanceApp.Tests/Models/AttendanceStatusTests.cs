using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class AttendanceStatusTests
{
    [Theory]
    [InlineData(AttendanceStatus.Attending, "参加")]
    [InlineData(AttendanceStatus.NotAttending, "不参加")]
    [InlineData(AttendanceStatus.Undecided, "未定")]
    public void ToDisplayName_ReturnsExpectedJapaneseLabel(AttendanceStatus status, string expected)
    {
        Assert.Equal(expected, status.ToDisplayName());
    }

    [Fact]
    public void ToColor_EachStatusHasADistinctColor()
    {
        AttendanceStatus[] statuses =
        [
            AttendanceStatus.Attending,
            AttendanceStatus.NotAttending,
            AttendanceStatus.Undecided
        ];

        var colors = statuses.Select(s => s.ToColor()).ToList();

        Assert.Equal(colors.Count, colors.Distinct().Count());
    }
}
