using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class PieceTests
{
    [Fact]
    public void PartsSummary_NoAssignments_ReturnsUnsetMessage()
    {
        var piece = new Piece { Id = "p1", Title = "曲A", PartAssignments = [] };

        Assert.Equal("パート未設定", piece.PartsSummary);
    }

    [Fact]
    public void PartsSummary_UndividedPart_ReturnsPartNameOnly()
    {
        var piece = new Piece
        {
            Id = "p1",
            Title = "曲A",
            PartAssignments =
            [
                new PiecePartAssignment { Part = PartType.Soprano, Division = PartDivision.None }
            ]
        };

        Assert.Equal("ソプラノ", piece.PartsSummary);
    }

    [Fact]
    public void PartsSummary_DividedPart_IncludesDivisionLabelInParentheses()
    {
        var piece = new Piece
        {
            Id = "p1",
            Title = "曲A",
            PartAssignments =
            [
                new PiecePartAssignment { Part = PartType.Alto, Division = PartDivision.UpperLower }
            ]
        };

        Assert.Equal("アルト（上・下に分割）", piece.PartsSummary);
    }

    [Fact]
    public void PartsSummary_MultipleAssignments_JoinsWithJapaneseComma()
    {
        var piece = new Piece
        {
            Id = "p1",
            Title = "曲A",
            PartAssignments =
            [
                new PiecePartAssignment { Part = PartType.Soprano, Division = PartDivision.None },
                new PiecePartAssignment { Part = PartType.Bass, Division = PartDivision.None }
            ]
        };

        Assert.Equal("ソプラノ、ベース", piece.PartsSummary);
    }
}
