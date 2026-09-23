using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class TimeOfDayExtensionsTests
{
    [Theory]
    [InlineData(TimeOfDay.Morning, "午前")]
    [InlineData(TimeOfDay.Afternoon, "午後")]
    [InlineData(TimeOfDay.Evening, "夜間")]
    public void ToDisplayName_ReturnsJapaneseLabelForEachTimeOfDay(TimeOfDay timeOfDay, string expected)
    {
        Assert.Equal(expected, timeOfDay.ToDisplayName());
    }

    [Fact]
    public void All_EnumeratesInMorningAfternoonEveningOrder()
    {
        Assert.Equal([TimeOfDay.Morning, TimeOfDay.Afternoon, TimeOfDay.Evening], TimeOfDayExtensions.All);
    }
}
