using System.Security.Cryptography;
using System.Text;

namespace IyokoraAttendanceApp.Services;

/// <summary>
/// Firestore に保存するメンバー氏名を AES-256-CBC で暗号化・復号する。
/// このアプリはログイン機能を持たず全端末が同じデータへアクセスするため、
/// 鍵は端末間で共有できるようアプリ内に固定で埋め込んでいる
/// （Firestore コンソール等で氏名が平文表示されるのを防ぐ難読化目的であり、
/// アプリのソースやビルド成果物を解析できる相手からの秘匿は保証しない）。
/// </summary>
public static class NameCipher
{
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("IyokoraAttendanceApp:Member.Name:v1"));

    /// <summary>氏名を暗号化し、IV を先頭に付与した Base64 文字列を返す。</summary>
    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String([.. aes.IV, .. cipherBytes]);
    }

    /// <summary>
    /// <see cref="Encrypt"/> で生成された文字列を復号する。
    /// 暗号化導入前に保存された平文データが残っている場合に備え、
    /// 復号できない値はそのまま平文として返す。
    /// </summary>
    public static string DecryptOrPlain(string value)
    {
        try
        {
            var buffer = Convert.FromBase64String(value);
            if (buffer.Length <= 16)
                return value;

            var iv = buffer[..16];
            var cipherBytes = buffer[16..];

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (FormatException)
        {
            return value;
        }
        catch (CryptographicException)
        {
            return value;
        }
    }
}
