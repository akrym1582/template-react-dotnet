using System.Security.Cryptography;

namespace Shared.Util;

/// <summary>
/// パスワードのハッシュ化と検証を行うユーティリティクラス。
/// PBKDF2（RFC 2898）を使用してパスワードを安全にハッシュ化する。
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// パスワードを PBKDF2 でハッシュ化し、"ソルト:ハッシュ" 形式の文字列を返す。
    /// </summary>
    /// <param name="password">ハッシュ化するパスワード（平文）。</param>
    /// <returns>"Base64(ソルト):Base64(ハッシュ)" 形式の文字列。</returns>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// パスワードとハッシュ文字列を比較し、一致するか検証する。
    /// </summary>
    /// <param name="password">検証するパスワード（平文）。</param>
    /// <param name="passwordHash"><see cref="Hash"/> で生成されたハッシュ文字列。</param>
    /// <returns>パスワードが一致する場合は <c>true</c>、それ以外は <c>false</c>。</returns>
    public static bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
}
