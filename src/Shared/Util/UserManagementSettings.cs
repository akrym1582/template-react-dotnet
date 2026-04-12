namespace Shared.Util;

/// <summary>
/// ユーザー管理に関する設定を保持するクラス。
/// appsettings.json の "UserManagement" セクションにバインドされる。
/// </summary>
public class UserManagementSettings
{
    /// <summary>マネージャーによるユーザー作成を許可するかどうか。</summary>
    public bool AllowManagerUserCreation { get; set; }

    /// <summary>新規ユーザー作成時の初期パスワード。</summary>
    public string InitialPassword { get; set; } = "Init@1234";

    /// <summary>パスワードポリシーの設定。</summary>
    public PasswordPolicySettings PasswordPolicy { get; set; } = new();
}

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
