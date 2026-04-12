namespace Shared.Util;

/// <summary>
/// パスワードポリシーの検証を行うユーティリティクラス。
/// </summary>
public static class PasswordPolicyHelper
{
    /// <summary>
    /// パスワードがポリシーを満たしているか検証する。
    /// </summary>
    /// <param name="password">検証するパスワード（平文）。</param>
    /// <param name="policy">検証に使用するパスワードポリシー設定。</param>
    /// <returns>ポリシー違反がある場合はエラーメッセージ、問題なければ <c>null</c>。</returns>
    public static string? Validate(string password, PasswordPolicySettings policy)
    {
        if (password.Length < policy.MinLength)
        {
            return $"パスワードは {policy.MinLength} 文字以上で入力してください。";
        }

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
        {
            return "パスワードには英大文字を 1 文字以上含めてください。";
        }

        if (policy.RequireLowercase && !password.Any(char.IsLower))
        {
            return "パスワードには英小文字を 1 文字以上含めてください。";
        }

        if (policy.RequireDigit && !password.Any(char.IsDigit))
        {
            return "パスワードには数字を 1 文字以上含めてください。";
        }

        if (policy.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return "パスワードには記号を 1 文字以上含めてください。";
        }

        return null;
    }
}
