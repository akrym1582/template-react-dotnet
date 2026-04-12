namespace WebApp.Options;

/// <summary>
/// テストログイン機能の設定を保持するクラス（開発環境専用）。
/// appsettings.json の "TestLogin" セクションにバインドされる。
/// </summary>
public class TestLoginOptions
{
    /// <summary>テストログインで使用できるユーザーの一覧。</summary>
    public List<TestLoginUserOption> Users { get; set; } = [];
}
