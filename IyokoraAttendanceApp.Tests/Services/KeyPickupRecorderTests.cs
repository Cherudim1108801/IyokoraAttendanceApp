using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class KeyPickupRecorderTests
{
    [Fact]
    public void ResolveRecordedName_KeyPickedUpTrue_ReturnsMemberName()
    {
        var result = KeyPickupRecorder.ResolveRecordedName(keyPickedUp: true, memberName: "山田太郎");

        Assert.Equal("山田太郎", result);
    }

    [Fact]
    public void ResolveRecordedName_KeyPickedUpFalse_ReturnsNull()
    {
        var result = KeyPickupRecorder.ResolveRecordedName(keyPickedUp: false, memberName: "山田太郎");

        Assert.Null(result);
    }
}
