namespace WebApp.Options;

/// <summary>
/// テストログイン用のユーザー設定を保持するクラス。
/// </summary>
public class TestLoginUserOption
{
    /// <summary>テストユーザーの ID。</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>テストユーザーに割り当てるロールの一覧。</summary>
    public List<string> Roles { get; set; } = [];
}
