using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class PracticeTests
{
    [Fact]
    public void PiecesSummary_NoPieces_ReturnsEmptyString()
    {
        var practice = new Practice { Id = "pr1", Pieces = [] };

        Assert.Equal(string.Empty, practice.PiecesSummary);
    }

    [Fact]
    public void PiecesSummary_MultiplePieces_JoinsTitlesWithJapaneseComma()
    {
        var practice = new Practice
        {
            Id = "pr1",
            Pieces =
            [
                new PracticePieceRef { PieceId = "p1", Title = "曲A" },
                new PracticePieceRef { PieceId = "p2", Title = "曲B" }
            ]
        };

        Assert.Equal("曲A、曲B", practice.PiecesSummary);
    }

    [Fact]
    public void TimeRangeSummary_BothTimesUnset_ReturnsEmptyString()
    {
        var practice = new Practice { Id = "pr1" };

        Assert.Equal(string.Empty, practice.TimeRangeSummary);
    }

    [Fact]
    public void TimeRangeSummary_OnlyStartTimeSet_ReturnsEmptyString()
    {
        var practice = new Practice { Id = "pr1", StartTime = "18:00" };

        Assert.Equal(string.Empty, practice.TimeRangeSummary);
    }

    [Fact]
    public void TimeRangeSummary_BothTimesSet_JoinsWithWaveDash()
    {
        var practice = new Practice { Id = "pr1", StartTime = "18:00", EndTime = "20:00" };

        Assert.Equal("18:00〜20:00", practice.TimeRangeSummary);
    }
}
