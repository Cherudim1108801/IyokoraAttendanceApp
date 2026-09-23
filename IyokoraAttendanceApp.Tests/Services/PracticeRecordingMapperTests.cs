using IyokoraAttendanceApp.Models;
using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class PracticeRecordingMapperTests
{
    [Fact]
    public void ToRecordings_NewSchema_ReadsAllRecordings()
    {
        var pieceFields = new Dictionary<string, object?>
        {
            ["pieceId"] = "piece-1",
            ["title"] = "曲A",
            ["recordings"] = new List<object?>
            {
                new Dictionary<string, object?> { ["id"] = "rec-1", ["name"] = "通し1回目", ["url"] = "https://example.com/1", ["featured"] = true },
                new Dictionary<string, object?> { ["id"] = "rec-2", ["name"] = "通し2回目", ["url"] = "https://example.com/2", ["featured"] = false }
            }
        };

        var recordings = PracticeRecordingMapper.ToRecordings(pieceFields);

        Assert.Equal(2, recordings.Count);
        Assert.Equal("rec-1", recordings[0].Id);
        Assert.Equal("通し1回目", recordings[0].Name);
        Assert.Equal("https://example.com/1", recordings[0].Url);
        Assert.True(recordings[0].IsFeatured);
        Assert.Equal("rec-2", recordings[1].Id);
        Assert.False(recordings[1].IsFeatured);
    }

    [Fact]
    public void ToRecordings_LegacySchemaWithUrl_FallsBackToSingleRecording()
    {
        var pieceFields = new Dictionary<string, object?>
        {
            ["pieceId"] = "piece-1",
            ["title"] = "曲A",
            ["recordingUrl"] = "https://example.com/legacy",
            ["featured"] = true
        };

        var recordings = PracticeRecordingMapper.ToRecordings(pieceFields);

        var recording = Assert.Single(recordings);
        Assert.False(string.IsNullOrEmpty(recording.Id));
        Assert.Equal("https://example.com/legacy", recording.Url);
        Assert.True(recording.IsFeatured);
        Assert.Equal(string.Empty, recording.Name);
    }

    [Fact]
    public void ToRecordings_LegacySchemaWithoutUrl_ReturnsEmpty()
    {
        var pieceFields = new Dictionary<string, object?>
        {
            ["pieceId"] = "piece-1",
            ["title"] = "曲A"
        };

        var recordings = PracticeRecordingMapper.ToRecordings(pieceFields);

        Assert.Empty(recordings);
    }

    [Fact]
    public void ToRecordings_RecordingsFieldPresentButEmpty_DoesNotFallBackToLegacy()
    {
        var pieceFields = new Dictionary<string, object?>
        {
            ["pieceId"] = "piece-1",
            ["title"] = "曲A",
            ["recordings"] = new List<object?>(),
            ["recordingUrl"] = "https://example.com/legacy-should-be-ignored"
        };

        var recordings = PracticeRecordingMapper.ToRecordings(pieceFields);

        Assert.Empty(recordings);
    }

    [Fact]
    public void ToRecordingFields_RoundTripsThroughToRecordings()
    {
        var original = new List<PracticeRecording>
        {
            new() { Id = "rec-1", Name = "通し", Url = "https://example.com/1", IsFeatured = true }
        };

        var fields = PracticeRecordingMapper.ToRecordingFields(original);
        var pieceFields = new Dictionary<string, object?> { ["recordings"] = fields };
        var roundTripped = PracticeRecordingMapper.ToRecordings(pieceFields);

        var recording = Assert.Single(roundTripped);
        Assert.Equal("rec-1", recording.Id);
        Assert.Equal("通し", recording.Name);
        Assert.Equal("https://example.com/1", recording.Url);
        Assert.True(recording.IsFeatured);
    }
}
