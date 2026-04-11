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

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _environment;
    private readonly TestLoginOptions _testLoginOptions;

    public AuthController(
        IServiceProvider serviceProvider,
        IOptions<TestLoginOptions> testLoginOptions,
        IWebHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _testLoginOptions = testLoginOptions.Value;
        _environment = environment;
    }

    /// <summary>
    /// Login with email and password, returns a cookie session.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Login([FromBody] LoginRequestDto request)
    {
        var user = await GetUserService().ValidateCredentialsAsync(request.Email, request.Password);
        if (user is null)
            return Unauthorized(new ApiResponseDto(false, "メールアドレスまたはパスワードが正しくありません。"));

        await SignInAsync(user);
        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    [HttpGet("test-users")]
    public ActionResult<ApiResponseDto<IReadOnlyList<TestLoginUserDto>>> GetTestUsers()
    {
        if (!_environment.IsDevelopment())
            return Ok(new ApiResponseDto<IReadOnlyList<TestLoginUserDto>>(true, []));

        var users = _testLoginOptions.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.UserId))
            .Select(user => new TestLoginUserDto(user.UserId.Trim(), NormalizeRoles(user.Roles)))
            .ToList();

        return Ok(new ApiResponseDto<IReadOnlyList<TestLoginUserDto>>(true, users));
    }

    [HttpPost("test-login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> TestLogin([FromBody] TestLoginRequestDto request)
    {
        if (!_environment.IsDevelopment())
            return NotFound(new ApiResponseDto(false, "テストログインは開発環境でのみ利用できます。"));

        var configuredUser = FindTestLoginUser(request.UserId);
        if (configuredUser is null)
            return Unauthorized(new ApiResponseDto(false, "テストログインユーザーが見つかりません。"));

        var user = ToTestLoginUserDto(configuredUser);
        await SignInAsync(user);
        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    /// <summary>
    /// Login with Azure Entra ID JWT token, returns a cookie session.
    /// Client obtains the JWT, then calls this endpoint to establish a cookie session.
    /// </summary>
    [HttpPost("entra-login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> EntraLogin([FromBody] EntraLoginRequestDto request)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(request.IdToken))
            return BadRequest(new ApiResponseDto(false, "無効なトークンです。"));

        var jwt = handler.ReadJwtToken(request.IdToken);
        var oid = jwt.Claims.FirstOrDefault(c => c.Type == "oid")?.Value;
        var email = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == "email")?.Value;
        var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (string.IsNullOrEmpty(oid) || string.IsNullOrEmpty(email))
            return BadRequest(new ApiResponseDto(false, "トークンに必要な情報が含まれていません。"));

        var user = await GetUserService().GetOrCreateEntraUserAsync(oid, email, name ?? email);
        await SignInAsync(user);

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponseDto>> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new ApiResponseDto(true, "ログアウトしました。"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Unauthorized(new ApiResponseDto(false));

        var configuredUser = FindTestLoginUser(userId);
        if (configuredUser is not null)
            return Ok(new ApiResponseDto<UserDto>(true, ToTestLoginUserDto(configuredUser)));

        var user = await GetUserService().GetByIdAsync(userId);

        if (user is null)
            return Unauthorized(new ApiResponseDto(false));

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

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
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
    }

    private TestLoginUserOption? FindTestLoginUser(string userId) =>
        _testLoginOptions.Users.FirstOrDefault(
            user => string.Equals(user.UserId?.Trim(), userId.Trim(), StringComparison.Ordinal));

    private static UserDto ToTestLoginUserDto(TestLoginUserOption user)
    {
        var userId = user.UserId.Trim();
        return new UserDto(
            UserId: userId,
            Email: $"{userId}@test.local",
            DisplayName: userId,
            Roles: NormalizeRoles(user.Roles),
            IsActive: true);
    }

    private static IReadOnlyList<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        var normalizedRoles = roles?
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalizedRoles is { Count: > 0 } ? normalizedRoles : [Constants.Roles.User];
    }

    private IUserService GetUserService() =>
        _serviceProvider.GetRequiredService<IUserService>();
}
