using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class PracticeScheduleValidatorTests
{
    [Fact]
    public void TryValidateTimeRange_BothNull_IsValid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(null, null, out var error);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidateTimeRange_OnlyStartSet_IsInvalid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(new TimeOnly(18, 0), null, out var error);

        Assert.False(isValid);
        Assert.Equal("開始時刻と終了時刻は両方入力してください。", error);
    }

    [Fact]
    public void TryValidateTimeRange_OnlyEndSet_IsInvalid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(null, new TimeOnly(20, 0), out var error);

        Assert.False(isValid);
        Assert.Equal("開始時刻と終了時刻は両方入力してください。", error);
    }

    [Fact]
    public void TryValidateTimeRange_MinuteNotMultipleOfTen_IsInvalid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(new TimeOnly(18, 5), new TimeOnly(20, 0), out var error);

        Assert.False(isValid);
        Assert.Equal("時刻は10分単位で入力してください。", error);
    }

    [Fact]
    public void TryValidateTimeRange_EndBeforeStart_IsInvalid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(new TimeOnly(20, 0), new TimeOnly(18, 0), out var error);

        Assert.False(isValid);
        Assert.Equal("終了時刻は開始時刻より後にしてください。", error);
    }

    [Fact]
    public void TryValidateTimeRange_EndEqualsStart_IsInvalid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(new TimeOnly(18, 0), new TimeOnly(18, 0), out var error);

        Assert.False(isValid);
        Assert.Equal("終了時刻は開始時刻より後にしてください。", error);
    }

    [Fact]
    public void TryValidateTimeRange_ValidTenMinuteRange_IsValid()
    {
        var isValid = PracticeScheduleValidator.TryValidateTimeRange(new TimeOnly(18, 0), new TimeOnly(18, 10), out var error);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void TryValidateTimelineItem_MissingTimes_IsInvalid()
    {
        var item = new TimelineItemInput { StartTime = null, EndTime = null, Content = "基礎合奏" };

        var isValid = PracticeScheduleValidator.TryValidateTimelineItem(item, out var error);

        Assert.False(isValid);
        Assert.Equal("タイムスケジュールの開始・終了時刻を入力してください。", error);
    }

    [Fact]
    public void TryValidateTimelineItem_ValidTimes_IsValid()
    {
        var item = new TimelineItemInput { StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(18, 10), Content = "基礎合奏" };

        var isValid = PracticeScheduleValidator.TryValidateTimelineItem(item, out var error);

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void ParseTimeOrNull_HHmmString_ReturnsTimeOnly()
    {
        var time = PracticeScheduleValidator.ParseTimeOrNull("18:30");

        Assert.Equal(new TimeOnly(18, 30), time);
    }

    [Theory]
    [InlineData("")]
    [InlineData("不正な値")]
    public void ParseTimeOrNull_EmptyOrInvalid_ReturnsNull(string value)
    {
        var time = PracticeScheduleValidator.ParseTimeOrNull(value);

        Assert.Null(time);
    }
}
