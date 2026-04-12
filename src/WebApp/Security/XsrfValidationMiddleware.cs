using Microsoft.AspNetCore.Antiforgery;
using Shared.Dto;

namespace WebApp.Security;

/// <summary>
/// 認証済みユーザーからの API リクエストに対して XSRF トークンを検証するミドルウェア。
/// ログインエンドポイントなど一部のパスは検証をスキップする。
/// </summary>
public class XsrfValidationMiddleware
{
    private static readonly PathString ApiPathPrefix = new("/api");

    private readonly RequestDelegate _next;

    /// <summary>
    /// <see cref="XsrfValidationMiddleware"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="next">パイプラインの次のミドルウェア。</param>
    public XsrfValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// リクエストを処理する。認証済みの API リクエストに対して XSRF トークンを検証する。
    /// </summary>
    /// <param name="context">現在の HTTP コンテキスト。</param>
    /// <param name="antiforgery">アンチフォージェリサービス。</param>
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (ShouldValidate(context))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ApiResponseDto(false, "XSRF トークンが無効です。"));
                return;
            }
        }

        await _next(context);
    }

    private static bool ShouldValidate(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
            return false;

        if (!context.Request.Path.StartsWithSegments(ApiPathPrefix))
            return false;

        return !IsIgnoredPath(context.Request.Path);
    }

    private static bool IsIgnoredPath(PathString path) =>
        path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/api/auth/entra-login", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/api/auth/test-login", StringComparison.OrdinalIgnoreCase)
        || path.Equals("/api/auth/reset-password-by-credentials", StringComparison.OrdinalIgnoreCase);
}
