using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Dto;
using Shared.Services;
using Shared.Util;
using WebApp.Options;
using WebApp.Security;

namespace WebApp.Controllers;

/// <summary>
/// 認証に関する API エンドポイントを提供するコントローラー。
/// ローカルログイン・Azure Entra ID ログイン・テストログイン・ログアウト・パスワード管理を担う。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly Lazy<IUserService> _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly TestLoginOptions _testLoginOptions;
    private readonly IXsrfTokenCookieService _xsrfTokenCookieService;

    /// <summary>
    /// <see cref="AuthController"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="userService">ユーザーサービス（循環依存回避のため遅延初期化）。</param>
    /// <param name="testLoginOptions">テストログインオプション。</param>
    /// <param name="environment">ホスト環境情報。</param>
    /// <param name="xsrfTokenCookieService">XSRF トークンクッキーサービス。</param>
    public AuthController(
        Lazy<IUserService> userService,
        IOptions<TestLoginOptions> testLoginOptions,
        IWebHostEnvironment environment,
        IXsrfTokenCookieService xsrfTokenCookieService)
    {
        _userService = userService;
        _testLoginOptions = testLoginOptions.Value;
        _environment = environment;
        _xsrfTokenCookieService = xsrfTokenCookieService;
    }

    /// <summary>
    /// メールアドレスとパスワードでログインし、クッキーセッションを発行する。
    /// </summary>
    /// <param name="request">ログインリクエスト（メールアドレスとパスワード）。</param>
    /// <returns>ログインに成功した場合はユーザー情報、失敗した場合は 401 を返す。</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Login([FromBody] LoginRequestDto request)
    {
        var user = await GetUserService().ValidateCredentialsAsync(request.Email, request.Password);
        if (user is null)
        {
            return Unauthorized(new ApiResponseDto(false, "メールアドレスまたはパスワードが正しくありません。"));
        }

        await SignInAsync(user);
        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    /// <summary>
    /// テストログイン用ユーザーの一覧を取得する（開発環境専用）。
    /// 本番環境では空のリストを返す。
    /// </summary>
    /// <returns>テストログイン用ユーザーの一覧。</returns>
    [HttpGet("test-users")]
    public ActionResult<ApiResponseDto<IReadOnlyList<TestLoginUserDto>>> GetTestUsers()
    {
        if (!_environment.IsDevelopment())
        {
            return Ok(new ApiResponseDto<IReadOnlyList<TestLoginUserDto>>(true, []));
        }

        var users = _testLoginOptions.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.UserId))
            .Select(user => new TestLoginUserDto(user.UserId.Trim(), RoleHelper.NormalizeRoles(user.Roles)))
            .ToList();

        return Ok(new ApiResponseDto<IReadOnlyList<TestLoginUserDto>>(true, users));
    }

    /// <summary>
    /// 指定したテストユーザーとしてログインする（開発環境専用）。
    /// </summary>
    /// <param name="request">テストログインリクエスト（ユーザー ID）。</param>
    /// <returns>ログインに成功した場合はユーザー情報、開発環境以外では 404、ユーザー未発見時は 401 を返す。</returns>
    [HttpPost("test-login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> TestLogin([FromBody] TestLoginRequestDto request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound(new ApiResponseDto(false, "テストログインは開発環境でのみ利用できます。"));
        }

        var configuredUser = FindTestLoginUser(request.UserId);
        if (configuredUser is null)
        {
            return Unauthorized(new ApiResponseDto(false, "テストログインユーザーが見つかりません。"));
        }

        var user = ToTestLoginUserDto(configuredUser);
        await SignInAsync(user);
        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    /// <summary>
    /// Azure Entra ID の JWT トークンで認証し、クッキーセッションを発行する。
    /// クライアントが取得した JWT をこのエンドポイントに送信することでセッションを確立する。
    /// </summary>
    /// <param name="request">Entra ID ログインリクエスト（JWT の ID トークン）。</param>
    /// <returns>ログインに成功した場合はユーザー情報、トークンが無効な場合は 400 を返す。</returns>
    [HttpPost("entra-login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> EntraLogin([FromBody] EntraLoginRequestDto request)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(request.IdToken))
        {
            return BadRequest(new ApiResponseDto(false, "無効なトークンです。"));
        }

        var jwt = handler.ReadJwtToken(request.IdToken);
        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (string.IsNullOrEmpty(oid) || string.IsNullOrEmpty(email))
        {
            return BadRequest(new ApiResponseDto(false, "トークンに必要な情報が含まれていません。"));
        }

        var user = await GetUserService().GetOrCreateEntraUserAsync(oid, email, name ?? email);
        await SignInAsync(user);

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    /// <summary>
    /// ログアウトしてクッキーセッションを削除する。
    /// </summary>
    /// <returns>ログアウト成功メッセージ。</returns>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponseDto>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _xsrfTokenCookieService.DeleteTokenCookies(HttpContext);
        return Ok(new ApiResponseDto(true, "ログアウトしました。"));
    }

    /// <summary>
    /// 現在ログイン中のユーザー情報を取得する。
    /// </summary>
    /// <returns>ログイン中のユーザー情報。未認証の場合は 401 を返す。</returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized(new ApiResponseDto(false));
        }

        var configuredUser = FindTestLoginUser(userId);
        if (configuredUser is not null)
        {
            return Ok(new ApiResponseDto<UserDto>(true, ToTestLoginUserDto(configuredUser)));
        }

        var user = await GetUserService().GetByIdAsync(userId);

        if (user is null)
        {
            return Unauthorized(new ApiResponseDto(false));
        }

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    /// <summary>
    /// ログイン中のユーザーのパスワードを変更する。
    /// </summary>
    /// <param name="request">パスワード変更リクエスト（新しいパスワード）。</param>
    /// <returns>変更後のユーザー情報。ポリシー違反時は 400、ユーザー未発見時は 404 を返す。</returns>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var validationMessage = await GetUserService().ValidatePasswordPolicyAsync(request.NewPassword);
        if (validationMessage is not null)
        {
            return BadRequest(new ApiResponseDto(false, validationMessage));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized(new ApiResponseDto(false, "認証情報を確認できません。"));
        }

        var user = await GetUserService().ChangePasswordAsync(userId, request.NewPassword);
        if (user is null)
        {
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));
        }

        await SignInAsync(user);
        return Ok(new ApiResponseDto<UserDto>(true, user, "パスワードを変更しました。"));
    }

    /// <summary>
    /// ログイン中のユーザーのパスワードを初期パスワードにリセットする。
    /// </summary>
    /// <returns>リセット後の初期パスワード情報。ユーザー未発見時は 404 を返す。</returns>
    [Authorize]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponseDto<PasswordResetResultDto>>> ResetPassword()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized(new ApiResponseDto(false, "認証情報を確認できません。"));
        }

        var resetResult = await GetUserService().ResetPasswordAsync(userId);
        if (resetResult is null)
        {
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));
        }

        return Ok(new ApiResponseDto<PasswordResetResultDto>(true, resetResult, "パスワードを初期化しました。"));
    }

    /// <summary>
    /// 現在のパスワードで認証を行い、パスワードを初期パスワードにリセットする。
    /// 未ログインユーザーが自身のパスワードをリセットする際に使用する。
    /// </summary>
    /// <param name="request">メールアドレスと現在のパスワードを含むリクエスト。</param>
    /// <returns>リセット後の初期パスワード情報。認証失敗時は 401、ユーザー未発見時は 404 を返す。</returns>
    [AllowAnonymous]
    [HttpPost("reset-password-by-credentials")]
    public async Task<ActionResult<ApiResponseDto<PasswordResetResultDto>>> ResetPasswordByCredentials(
        [FromBody] ResetPasswordByCredentialsRequestDto request)
    {
        var user = await GetUserService().ValidateCredentialsAsync(request.Email, request.CurrentPassword);
        if (user is null)
        {
            return Unauthorized(new ApiResponseDto(false, "メールアドレスまたはパスワードが正しくありません。"));
        }

        var resetResult = await GetUserService().ResetPasswordAsync(user.UserId);
        if (resetResult is null)
        {
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));
        }

        return Ok(new ApiResponseDto<PasswordResetResultDto>(true, resetResult, "パスワードを初期化しました。"));
    }

    private static UserDto ToTestLoginUserDto(TestLoginUserOption user)
    {
        var userId = user.UserId.Trim();
        return new UserDto(
            UserId: userId,
            Email: $"{userId}@test.local",
            DisplayName: userId,
            StoreCode: string.Empty,
            StoreName: string.Empty,
            Roles: RoleHelper.NormalizeRoles(user.Roles),
            IsActive: true,
            MustChangePassword: false);
    }

    private IUserService GetUserService() =>
        _userService.Value;

    private async Task SignInAsync(UserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            });

        _xsrfTokenCookieService.RefreshTokenCookie(HttpContext);
    }

    private TestLoginUserOption? FindTestLoginUser(string userId)
    {
        var trimmedUserId = userId.Trim();

        return _testLoginOptions.Users.FirstOrDefault(
            user => string.Equals(user.UserId?.Trim(), trimmedUserId, StringComparison.Ordinal));
    }
}
