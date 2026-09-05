using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class PartTypeTests
{
    [Theory]
    [InlineData(PartType.Soprano, "ソプラノ")]
    [InlineData(PartType.Alto, "アルト")]
    [InlineData(PartType.Tenor, "テナー")]
    [InlineData(PartType.Bass, "ベース")]
    public void ToDisplayName_ReturnsExpectedJapaneseLabel(PartType part, string expected)
    {
        Assert.Equal(expected, part.ToDisplayName());
    }

    [Fact]
    public void All_ContainsEveryPartInSopranoToBassOrder()
    {
        Assert.Equal(
            [PartType.Soprano, PartType.Alto, PartType.Tenor, PartType.Bass],
            PartTypeExtensions.All);
    }

    [Fact]
    public void ToColor_EachPartHasADistinctColor()
    {
        var colors = PartTypeExtensions.All.Select(p => p.ToColor()).ToList();

        Assert.Equal(colors.Count, colors.Distinct().Count());
    }

    [Fact]
    public void ToCardBackgroundColor_EachPartHasADistinctColor()
    {
        var colors = PartTypeExtensions.All.Select(p => p.ToCardBackgroundColor()).ToList();

        Assert.Equal(colors.Count, colors.Distinct().Count());
    }
}
