using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class LoginIdGeneratorTests
{
    [Fact]
    public void Format_PadsNumberToFourDigits()
    {
        var id = LoginIdGenerator.Format("IK", 7);

        Assert.Equal("IK0007", id);
    }

    [Fact]
    public void GenerateCandidate_AlwaysProducesValidFormat()
    {
        for (var i = 0; i < 100; i++)
        {
            var candidate = LoginIdGenerator.GenerateCandidate("IK");

            Assert.StartsWith("IK", candidate);
            Assert.Equal(6, candidate.Length);
            Assert.True(LoginIdGenerator.IsValidFormat(candidate));
        }
    }

    [Theory]
    [InlineData("IK0001", true)]
    [InlineData("ABCD9999", true)]
    [InlineData("IK001", false)]
    [InlineData("00001234", false)]
    [InlineData("IK12A4", false)]
    [InlineData("1234", false)]
    [InlineData("", false)]
    [InlineData("ik0001", false)]
    public void IsValidFormat_OnlyUppercaseLetterPrefixPlusFourDigits_IsValid(string value, bool expected)
    {
        Assert.Equal(expected, LoginIdGenerator.IsValidFormat(value));
    }

    [Fact]
    public void Normalize_TrimsWhitespaceAndUppercasesLetters()
    {
        var normalized = LoginIdGenerator.Normalize("  ik0001  ");

        Assert.Equal("IK0001", normalized);
    }

    [Theory]
    [InlineData(0, "IK0000")]
    [InlineData(9999, "IK9999")]
    public void Format_BoundaryNumbers_PadCorrectly(int number, string expected)
    {
        Assert.Equal(expected, LoginIdGenerator.Format("IK", number));
    }

    [Fact]
    public void ResolveUnique_FirstCandidateNotTaken_ReturnsItImmediately()
    {
        var attempts = 0;
        var result = LoginIdGenerator.ResolveUnique(
            candidateFactory: () => { attempts++; return "IK0001"; },
            isTaken: _ => false);

        Assert.Equal("IK0001", result);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public void ResolveUnique_RegeneratesUntilCandidateIsNotTaken()
    {
        var candidates = new Queue<string>(["IK0001", "IK0002", "IK0003"]);
        var takenIds = new HashSet<string> { "IK0001", "IK0002" };

        var result = LoginIdGenerator.ResolveUnique(
            candidateFactory: candidates.Dequeue,
            isTaken: takenIds.Contains);

        Assert.Equal("IK0003", result);
    }

    [Fact]
    public void ResolveUnique_ExceedsMaxAttempts_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => LoginIdGenerator.ResolveUnique(
            candidateFactory: () => "IK0001",
            isTaken: _ => true,
            maxAttempts: 5));
    }
}
