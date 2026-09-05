using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class FirestoreDocumentTests
{
    private static FirestoreDocument CreateDocument(Dictionary<string, object?> fields) =>
        new() { Id = "doc-1", Fields = fields };

    [Fact]
    public void GetString_FieldPresentWithCorrectType_ReturnsValue()
    {
        var doc = CreateDocument(new() { ["name"] = "アリス" });

        Assert.Equal("アリス", doc.GetString("name"));
    }

    [Fact]
    public void GetString_FieldMissing_ReturnsFallback()
    {
        var doc = CreateDocument([]);

        Assert.Equal("既定値", doc.GetString("name", "既定値"));
    }

    [Fact]
    public void GetString_FieldWrongType_ReturnsFallback()
    {
        var doc = CreateDocument(new() { ["name"] = 123L });

        Assert.Equal("", doc.GetString("name"));
    }

    [Fact]
    public void GetLong_FieldPresentWithCorrectType_ReturnsValue()
    {
        var doc = CreateDocument(new() { ["count"] = 42L });

        Assert.Equal(42L, doc.GetLong("count"));
    }

    [Fact]
    public void GetLong_FieldMissing_ReturnsFallback()
    {
        var doc = CreateDocument([]);

        Assert.Equal(-1L, doc.GetLong("count", -1L));
    }

    [Fact]
    public void GetDateTime_FieldPresentWithCorrectType_ReturnsValue()
    {
        var expected = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var doc = CreateDocument(new() { ["updatedAt"] = expected });

        Assert.Equal(expected, doc.GetDateTime("updatedAt"));
    }

    [Fact]
    public void GetDateTime_FieldMissing_ReturnsFallback()
    {
        var fallback = new DateTime(2000, 1, 1);
        var doc = CreateDocument([]);

        Assert.Equal(fallback, doc.GetDateTime("updatedAt", fallback));
    }

    [Fact]
    public void GetBool_FieldPresentWithCorrectType_ReturnsValue()
    {
        var doc = CreateDocument(new() { ["isFeatured"] = true });

        Assert.True(doc.GetBool("isFeatured"));
    }

    [Fact]
    public void GetBool_FieldMissing_ReturnsFallback()
    {
        var doc = CreateDocument([]);

        Assert.True(doc.GetBool("isFeatured", true));
    }

    [Fact]
    public void GetList_FieldPresentWithCorrectType_ReturnsValue()
    {
        List<object?> expected = ["a", "b"];
        var doc = CreateDocument(new() { ["items"] = expected });

        Assert.Equal(expected, doc.GetList("items"));
    }

    [Fact]
    public void GetList_FieldMissing_ReturnsEmptyList()
    {
        var doc = CreateDocument([]);

        Assert.Empty(doc.GetList("items"));
    }
}
