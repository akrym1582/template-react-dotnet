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
