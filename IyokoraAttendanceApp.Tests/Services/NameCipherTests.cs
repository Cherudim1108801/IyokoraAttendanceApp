using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class NameCipherTests
{
    [Theory]
    [InlineData("田中太郎")]
    [InlineData("")]
    [InlineData("A")]
    public void Encrypt_ThenDecryptOrPlain_ReturnsOriginalValue(string plainText)
    {
        var encrypted = NameCipher.Encrypt(plainText);

        var decrypted = NameCipher.DecryptOrPlain(encrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_SameInputTwice_ProducesDifferentCiphertext()
    {
        const string plainText = "鈴木花子";

        var first = NameCipher.Encrypt(plainText);
        var second = NameCipher.Encrypt(plainText);

        Assert.NotEqual(first, second);
        Assert.Equal(plainText, NameCipher.DecryptOrPlain(first));
        Assert.Equal(plainText, NameCipher.DecryptOrPlain(second));
    }

    [Fact]
    public void DecryptOrPlain_NonBase64LegacyPlainText_ReturnsValueUnchanged()
    {
        const string legacyPlainText = "山田次郎";

        var result = NameCipher.DecryptOrPlain(legacyPlainText);

        Assert.Equal(legacyPlainText, result);
    }

    [Fact]
    public void DecryptOrPlain_Base64ButTooShortToContainIv_ReturnsValueUnchanged()
    {
        var tooShort = Convert.ToBase64String(new byte[10]);

        var result = NameCipher.DecryptOrPlain(tooShort);

        Assert.Equal(tooShort, result);
    }
}
