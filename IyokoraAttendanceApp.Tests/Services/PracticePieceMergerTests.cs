using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class PracticePieceMergerTests
{
    private static PracticePieceRef CreatePiece(string id, string title, params PracticeRecording[] recordings) => new()
    {
        PieceId = id,
        Title = title,
        Recordings = [.. recordings]
    };

    private static PracticeRecording CreateRecording(string id) => new() { Id = id, Url = $"https://example.com/{id}" };

    [Fact]
    public void MergeSelectedPieces_StillSelectedPiece_KeepsExistingRecordings()
    {
        var recording = CreateRecording("rec-1");
        var existing = new List<PracticePieceRef> { CreatePiece("A", "曲A", recording) };
        var selected = new List<(string PieceId, string Title)> { ("A", "曲A") };

        var merged = PracticePieceMerger.MergeSelectedPieces(existing, selected);

        var piece = Assert.Single(merged);
        Assert.Equal("A", piece.PieceId);
        Assert.Same(recording, Assert.Single(piece.Recordings));
    }

    [Fact]
    public void MergeSelectedPieces_NewlySelectedPiece_AddedWithEmptyRecordings()
    {
        var existing = new List<PracticePieceRef>();
        var selected = new List<(string PieceId, string Title)> { ("B", "曲B") };

        var merged = PracticePieceMerger.MergeSelectedPieces(existing, selected);

        var piece = Assert.Single(merged);
        Assert.Equal("B", piece.PieceId);
        Assert.Equal("曲B", piece.Title);
        Assert.Empty(piece.Recordings);
    }

    [Fact]
    public void MergeSelectedPieces_DeselectedPiece_IsExcluded()
    {
        var existing = new List<PracticePieceRef>
        {
            CreatePiece("A", "曲A", CreateRecording("rec-1")),
            CreatePiece("B", "曲B")
        };
        var selected = new List<(string PieceId, string Title)> { ("A", "曲A") };

        var merged = PracticePieceMerger.MergeSelectedPieces(existing, selected);

        Assert.Equal(["A"], merged.Select(p => p.PieceId));
    }

    [Fact]
    public void MergeSelectedPieces_NoneSelected_ReturnsEmpty()
    {
        var existing = new List<PracticePieceRef> { CreatePiece("A", "曲A", CreateRecording("rec-1")) };
        var selected = new List<(string PieceId, string Title)>();

        var merged = PracticePieceMerger.MergeSelectedPieces(existing, selected);

        Assert.Empty(merged);
    }
}
