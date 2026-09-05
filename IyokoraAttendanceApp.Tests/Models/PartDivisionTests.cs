using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class PartDivisionTests
{
    [Theory]
    [InlineData(PartDivision.None, "分割しない")]
    [InlineData(PartDivision.UpperLower, "上・下に分割")]
    public void ToDisplayName_ReturnsExpectedJapaneseLabel(PartDivision division, string expected)
    {
        Assert.Equal(expected, division.ToDisplayName());
    }

    [Fact]
    public void ToSubPartLabels_None_ReturnsSinglePartNameLabel()
    {
        var labels = PartDivision.None.ToSubPartLabels(PartType.Soprano);

        Assert.Equal(["ソプラノ"], labels);
    }

    [Fact]
    public void ToSubPartLabels_UpperLower_ReturnsUpperAndLowerLabels()
    {
        var labels = PartDivision.UpperLower.ToSubPartLabels(PartType.Alto);

        Assert.Equal(["アルト上", "アルト下"], labels);
    }

    [Fact]
    public void All_ContainsEveryDivisionInNoneFirstOrder()
    {
        Assert.Equal([PartDivision.None, PartDivision.UpperLower], PartDivisionExtensions.All);
    }
}
