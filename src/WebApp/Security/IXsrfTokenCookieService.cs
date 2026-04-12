namespace WebApp.Security;

public interface IXsrfTokenCookieService
{
    void RefreshTokenCookie(HttpContext httpContext);
    void DeleteTokenCookies(HttpContext httpContext);
}
