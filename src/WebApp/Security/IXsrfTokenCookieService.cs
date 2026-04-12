namespace WebApp.Security;

/// <summary>
/// XSRF トークンクッキーの発行・削除を行うサービスインターフェース。
/// </summary>
public interface IXsrfTokenCookieService
{
    /// <summary>
    /// XSRF トークンクッキーを更新（再発行）する。
    /// </summary>
    /// <param name="httpContext">現在の HTTP コンテキスト。</param>
    void RefreshTokenCookie(HttpContext httpContext);

    /// <summary>
    /// XSRF トークンクッキーおよびアンチフォージェリクッキーを削除する。
    /// </summary>
    /// <param name="httpContext">現在の HTTP コンテキスト。</param>
    void DeleteTokenCookies(HttpContext httpContext);
}
