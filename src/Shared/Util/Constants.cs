namespace Shared.Util;

/// <summary>
/// アプリケーション全体で使用する定数を定義する静的クラス。
/// </summary>
public static class Constants
{
    /// <summary>ユーザーエンティティの Azure Table Storage パーティションキー。</summary>
    public const string UserPartitionKey = "USER";

    /// <summary>ユーザーテーブルの名前。</summary>
    public const string UsersTableName = "Users";

    /// <summary>認証クッキーの名前。</summary>
    public const string AuthCookieName = ".TemplateApp.Auth";

    /// <summary>CSRF 対策用アンチフォージェリクッキーの名前。</summary>
    public const string AntiforgeryCookieName = ".TemplateApp.Antiforgery";

    /// <summary>クライアントが読み取る XSRF トークンクッキーの名前。</summary>
    public const string XsrfTokenCookieName = "XSRF-TOKEN";

    /// <summary>クライアントがリクエストヘッダーに含める XSRF トークンのヘッダー名。</summary>
    public const string XsrfHeaderName = "X-XSRF-TOKEN";

    /// <summary>
    /// ロール定数を定義する静的クラス。
    /// </summary>
    public static class Roles
    {
        /// <summary>一般ユーザーロール。</summary>
        public const string General = "general";

        /// <summary>マネージャーロール。</summary>
        public const string Manager = "manager";

        /// <summary>特権ユーザーロール。</summary>
        public const string Privileged = "privileged";

        /// <summary>管理可能なロールのカンマ区切り文字列。</summary>
        public const string ManageableRolesCsv = $"{Manager},{Privileged}";
    }
}
