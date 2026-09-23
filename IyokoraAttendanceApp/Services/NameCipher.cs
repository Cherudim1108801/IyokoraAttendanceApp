using System.Security.Cryptography;
using System.Text;

namespace IyokoraAttendanceApp.Services;

/// <summary>
/// Firestore に保存するメンバー氏名を AES-256-GCM（認証付き暗号）で暗号化・復号する。
/// このアプリはログイン機能を持たず全端末が同じデータへアクセスするため、
/// 鍵は端末間で共有できるようアプリ内に固定で埋め込んでいる
/// （Firestore コンソール等で氏名が平文表示されるのを防ぐ難読化目的であり、
/// アプリのソースやビルド成果物を解析できる相手からの秘匿は保証しない）。
///
/// 暗号文の形式（Base64(IV(12byte) + 暗号文 + 認証タグ(16byte))）は、姉妹アプリである
/// Blazor WebAssembly 版（wwwroot/js/nameCipher.js、ブラウザの SubtleCrypto 経由）と
/// 同一のバイトレイアウトになるよう合わせてあり、同じ Firestore の値を双方から復号できる。
///
/// 以前は AES-CBC（改ざん検知なし）を使用していたが GCM に移行した。
/// <see cref="DecryptOrPlain"/> は旧 CBC 形式（IV16byte）で暗号化済みの既存データも
/// 引き続き復号でき、その場合は呼び出し元が GCM への移行（再暗号化）を判断できるよう
/// <see cref="NameDecryptResult.WasLegacyFormat"/> で通知する。
/// </summary>
public static class NameCipher
{
    private const int GcmNonceLength = 12;
    private const int GcmTagLength = 16;
    private const int CbcIvLength = 16;

    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("IyokoraAttendanceApp:Member.Name:v1"));

    /// <summary>氏名を AES-256-GCM で暗号化し、IV を先頭・認証タグを末尾に付与した Base64 文字列を返す。</summary>
    public static string Encrypt(string plainText)
    {
        var nonce = RandomNumberGenerator.GetBytes(GcmNonceLength);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[GcmTagLength];

        using (var aesGcm = new AesGcm(Key, GcmTagLength))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        return Convert.ToBase64String([.. nonce, .. cipherBytes, .. tag]);
    }

    /// <summary>
    /// <see cref="Encrypt"/> で生成された文字列、または旧 AES-CBC 形式の文字列を復号する。
    /// 暗号化導入前に保存された平文データが残っている場合に備え、
    /// どちらの方式でも復号できない値はそのまま平文として返す。
    /// </summary>
    public static NameDecryptResult DecryptOrPlain(string value)
    {
        byte[] buffer;
        try
        {
            buffer = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return new NameDecryptResult(value, WasLegacyFormat: false);
        }

        if (TryDecryptGcm(buffer, out var gcmPlain))
            return new NameDecryptResult(gcmPlain, WasLegacyFormat: false);

        if (TryDecryptLegacyCbc(buffer, out var cbcPlain))
            return new NameDecryptResult(cbcPlain, WasLegacyFormat: true);

        // 暗号化導入前に保存された平文データの可能性があるため、そのまま返す。
        return new NameDecryptResult(value, WasLegacyFormat: false);
    }

    private static bool TryDecryptGcm(byte[] buffer, out string plainText)
    {
        plainText = string.Empty;
        if (buffer.Length < GcmNonceLength + GcmTagLength)
            return false;

        var nonce = buffer[..GcmNonceLength];
        var tag = buffer[^GcmTagLength..];
        var cipherBytes = buffer[GcmNonceLength..^GcmTagLength];
        var plainBytes = new byte[cipherBytes.Length];

        try
        {
            using var aesGcm = new AesGcm(Key, GcmTagLength);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            plainText = Encoding.UTF8.GetString(plainBytes);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static bool TryDecryptLegacyCbc(byte[] buffer, out string plainText)
    {
        plainText = string.Empty;
        if (buffer.Length <= CbcIvLength)
            return false;

        var iv = buffer[..CbcIvLength];
        var cipherBytes = buffer[CbcIvLength..];

        try
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            plainText = Encoding.UTF8.GetString(plainBytes);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}

/// <summary>
/// <see cref="NameCipher.DecryptOrPlain"/> の結果。
/// <paramref name="WasLegacyFormat"/> が true の場合、旧 AES-CBC 形式から復号されたことを示す
/// （呼び出し元は必要に応じて AES-GCM への再暗号化・保存し直しを検討する）。
/// </summary>
public readonly record struct NameDecryptResult(string Value, bool WasLegacyFormat);
