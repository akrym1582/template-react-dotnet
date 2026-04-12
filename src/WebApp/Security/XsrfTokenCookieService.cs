using Microsoft.AspNetCore.Antiforgery;
using Shared.Util;

namespace WebApp.Security;

/// <summary>
/// <see cref="IXsrfTokenCookieService"/> の実装クラス。
/// アンチフォージェリトークンをクッキーとして発行・削除する。
/// </summary>
public class XsrfTokenCookieService : IXsrfTokenCookieService
{
    private static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(7);

    private readonly IAntiforgery _antiforgery;

    /// <summary>
    /// <see cref="XsrfTokenCookieService"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="antiforgery">アンチフォージェリサービス。</param>
    public XsrfTokenCookieService(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    /// <inheritdoc/>
    public void RefreshTokenCookie(HttpContext httpContext)
    {
        var tokens = _antiforgery.GetAndStoreTokens(httpContext);
        if (string.IsNullOrEmpty(tokens.RequestToken))
            return;

        httpContext.Response.Cookies.Append(
            Constants.XsrfTokenCookieName,
            tokens.RequestToken,
            CreateCookieOptions(httpContext));
    }

    /// <inheritdoc/>
    public void DeleteTokenCookies(HttpContext httpContext)
    {
        var cookieOptions = CreateDeleteCookieOptions(httpContext);
        httpContext.Response.Cookies.Delete(Constants.XsrfTokenCookieName, cookieOptions);
        httpContext.Response.Cookies.Delete(Constants.AntiforgeryCookieName, cookieOptions);
    }

    private static CookieOptions CreateCookieOptions(HttpContext httpContext) =>
        new()
        {
            Path = "/",
            HttpOnly = false,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.Add(CookieLifetime)
        };

    private static CookieOptions CreateDeleteCookieOptions(HttpContext httpContext) =>
        new()
        {
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps
        };
}
