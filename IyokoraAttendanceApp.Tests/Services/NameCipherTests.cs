using System.Security.Cryptography;
using System.Text;
using IyokoraAttendanceApp.Services;
using Xunit;

namespace IyokoraAttendanceApp.Tests.Services;

public class NameCipherTests
{
    // NameCipher と同じ鍵導出。移行前の旧 AES-CBC 形式の暗号文を再現するためのテスト専用ヘルパー。
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("IyokoraAttendanceApp:Member.Name:v1"));

    private static string EncryptLegacyCbc(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String([.. aes.IV, .. cipherBytes]);
    }

    [Theory]
    [InlineData("田中太郎")]
    [InlineData("")]
    [InlineData("A")]
    public void Encrypt_ThenDecryptOrPlain_ReturnsOriginalValue(string plainText)
    {
        var encrypted = NameCipher.Encrypt(plainText);

        var decrypted = NameCipher.DecryptOrPlain(encrypted);

        Assert.Equal(plainText, decrypted.Value);
        Assert.False(decrypted.WasLegacyFormat);
    }

    [Fact]
    public void Encrypt_SameInputTwice_ProducesDifferentCiphertext()
    {
        const string plainText = "鈴木花子";

        var first = NameCipher.Encrypt(plainText);
        var second = NameCipher.Encrypt(plainText);

        Assert.NotEqual(first, second);
        Assert.Equal(plainText, NameCipher.DecryptOrPlain(first).Value);
        Assert.Equal(plainText, NameCipher.DecryptOrPlain(second).Value);
    }

    [Fact]
    public void DecryptOrPlain_LegacyCbcCiphertext_DecryptsAndReportsLegacyFormat()
    {
        const string plainText = "佐藤一郎";
        var legacyEncrypted = EncryptLegacyCbc(plainText);

        var result = NameCipher.DecryptOrPlain(legacyEncrypted);

        Assert.Equal(plainText, result.Value);
        Assert.True(result.WasLegacyFormat);
    }

    [Fact]
    public void DecryptOrPlain_NonBase64LegacyPlainText_ReturnsValueUnchanged()
    {
        const string legacyPlainText = "山田次郎";

        var result = NameCipher.DecryptOrPlain(legacyPlainText);

        Assert.Equal(legacyPlainText, result.Value);
        Assert.False(result.WasLegacyFormat);
    }

    [Fact]
    public void DecryptOrPlain_Base64ButTooShortToContainIv_ReturnsValueUnchanged()
    {
        var tooShort = Convert.ToBase64String(new byte[10]);

        var result = NameCipher.DecryptOrPlain(tooShort);

        Assert.Equal(tooShort, result.Value);
        Assert.False(result.WasLegacyFormat);
    }

    [Fact]
    public void DecryptOrPlain_TamperedGcmCiphertext_FailsAuthenticationAndReturnsValueUnchanged()
    {
        var encrypted = NameCipher.Encrypt("改ざんテスト");
        var bytes = Convert.FromBase64String(encrypted);
        bytes[^1] ^= 0xFF; // 認証タグの末尾1byteを破壊する
        var tampered = Convert.ToBase64String(bytes);

        var result = NameCipher.DecryptOrPlain(tampered);

        Assert.Equal(tampered, result.Value);
        Assert.False(result.WasLegacyFormat);
    }
}
