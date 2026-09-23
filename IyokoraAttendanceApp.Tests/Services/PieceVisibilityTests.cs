using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class PieceVisibilityTests
{
    private static Piece CreatePiece(string id, bool isArchived) => new()
    {
        Id = id,
        Title = id,
        IsArchived = isArchived
    };

    [Fact]
    public void Filter_IncludeArchivedFalse_ExcludesArchivedPieces()
    {
        var pieces = new[] { CreatePiece("A", isArchived: false), CreatePiece("B", isArchived: true) };

        var visible = PieceVisibility.Filter(pieces, includeArchived: false).ToList();

        Assert.Equal(["A"], visible.Select(p => p.Id));
    }

    [Fact]
    public void Filter_IncludeArchivedTrue_ReturnsAllPieces()
    {
        var pieces = new[] { CreatePiece("A", isArchived: false), CreatePiece("B", isArchived: true) };

        var visible = PieceVisibility.Filter(pieces, includeArchived: true).ToList();

        Assert.Equal(["A", "B"], visible.Select(p => p.Id));
    }
}
