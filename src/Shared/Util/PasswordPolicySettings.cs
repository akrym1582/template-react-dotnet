namespace Shared.Util;

/// <summary>
/// パスワードポリシーの設定を保持するクラス。
/// </summary>
public class PasswordPolicySettings
{
    /// <summary>パスワードの最小文字数。</summary>
    public int MinLength { get; set; } = 8;

    /// <summary>英大文字を必須とするかどうか。</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>英小文字を必須とするかどうか。</summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>数字を必須とするかどうか。</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>記号を必須とするかどうか。</summary>
    public bool RequireSpecialCharacter { get; set; } = true;
}
