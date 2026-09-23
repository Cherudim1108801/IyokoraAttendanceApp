using IyokoraAttendanceApp.Models;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Models;

public class ScheduleVoteTests
{
    [Fact]
    public void BuildId_CombinesCandidateIdAndMemberId()
    {
        var id = ScheduleVote.BuildId("candidate1", "member1");

        Assert.Equal("candidate1_member1", id);
    }
}
