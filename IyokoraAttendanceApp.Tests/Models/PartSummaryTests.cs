using IyokoraAttendanceApp.Models;
using Microsoft.Maui.Graphics;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class PartSummaryTests
{
    [Fact]
    public void CountLabel_FormatsAttendingOverMemberCount()
    {
        var summary = new PartSummary
        {
            Part = PartType.Soprano,
            Label = "ソプラノ",
            Color = Colors.Red,
            CardBackgroundColor = Colors.Pink,
            AttendingCount = 3,
            MemberCount = 5,
            AttendeeNames = ["Aさん", "Bさん", "Cさん"]
        };

        Assert.Equal("3 / 5 人", summary.CountLabel);
    }
}
