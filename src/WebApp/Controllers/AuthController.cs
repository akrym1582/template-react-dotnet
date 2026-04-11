using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto;
using Shared.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Login with email and password, returns a cookie session.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _userService.ValidateCredentialsAsync(request.Email, request.Password);
        if (user is null)
            return Unauthorized(new ApiResponseDto(false, "メールアドレスまたはパスワードが正しくありません。"));

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

        var user = await _userService.GetOrCreateEntraUserAsync(oid, email, name ?? email);
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

        var user = await _userService.GetByIdAsync(userId);
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
}
