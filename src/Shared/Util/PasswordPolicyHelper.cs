namespace Shared.Util;

public static class PasswordPolicyHelper
{
    public static string? Validate(string password, PasswordPolicySettings policy)
    {
        if (password.Length < policy.MinLength)
            return $"パスワードは {policy.MinLength} 文字以上で入力してください。";

        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            return "パスワードには英大文字を 1 文字以上含めてください。";

        if (policy.RequireLowercase && !password.Any(char.IsLower))
            return "パスワードには英小文字を 1 文字以上含めてください。";

        if (policy.RequireDigit && !password.Any(char.IsDigit))
            return "パスワードには数字を 1 文字以上含めてください。";

        if (policy.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            return "パスワードには記号を 1 文字以上含めてください。";

        return null;
    }
}
