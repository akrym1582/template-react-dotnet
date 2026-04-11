using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto;
using Shared.Services;
using Shared.Util;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManagementSettings _settings;

    public UserController(IUserService userService, UserManagementSettings settings)
    {
        _userService = userService;
        _settings = settings;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<UserListResponseDto>>> GetAll()
    {
        if (!RoleHelper.CanManageUsers(User))
            return Forbid();

        var users = await _userService.GetAllAsync();
        return Ok(new ApiResponseDto<UserListResponseDto>(
            true,
            new UserListResponseDto(users, _settings.AllowManagerUserCreation)));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetById(string userId)
    {
        if (!CanAccessUser(userId))
            return Forbid();

        var user = await _userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CreatedUserResultDto>>> Create([FromBody] CreateUserRequestDto request)
    {
        if (!RoleHelper.CanManageUsers(User))
            return Forbid();

        if (!_settings.AllowManagerUserCreation)
            return Forbid();

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser is not null)
            return BadRequest(new ApiResponseDto(false, "同じメールアドレスのユーザーが存在します。"));

        var user = await _userService.CreateAsync(
            request.Email,
            request.DisplayName,
            _settings.InitialPassword,
            request.StoreCode,
            request.StoreName,
            request.Roles,
            mustChangePassword: true);

        return Ok(new ApiResponseDto<CreatedUserResultDto>(
            true,
            new CreatedUserResultDto(user, _settings.InitialPassword, true),
            "ユーザーを作成しました。"));
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Update(string userId, [FromBody] UpdateUserRequestDto request)
    {
        var isSelf = IsCurrentUser(userId);
        var canManageUsers = RoleHelper.CanManageUsers(User);

        if (!isSelf && !canManageUsers)
            return Forbid();

        if (!canManageUsers && request.Roles is { Count: > 0 })
            return Forbid();

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser is not null && !string.Equals(existingUser.UserId, userId, StringComparison.Ordinal))
            return BadRequest(new ApiResponseDto(false, "同じメールアドレスのユーザーが存在します。"));

        var currentUser = await _userService.GetByIdAsync(userId);
        if (currentUser is null)
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));

        var updatedUser = await _userService.UpdateAsync(
            userId,
            request.Email,
            request.DisplayName,
            canManageUsers ? request.StoreCode : currentUser.StoreCode,
            canManageUsers ? request.StoreName : currentUser.StoreName,
            canManageUsers ? request.Roles : currentUser.Roles);

        if (updatedUser is null)
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));

        return Ok(new ApiResponseDto<UserDto>(true, updatedUser, "ユーザー情報を更新しました。"));
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<ApiResponseDto>> Delete(string userId)
    {
        if (!RoleHelper.CanManageUsers(User))
            return Forbid();

        await _userService.DeleteAsync(userId);
        return Ok(new ApiResponseDto(true, "削除しました。"));
    }

    private bool CanAccessUser(string userId) =>
        IsCurrentUser(userId) || RoleHelper.CanManageUsers(User);

    private bool IsCurrentUser(string userId) =>
        string.Equals(User.FindFirstValue(ClaimTypes.NameIdentifier), userId, StringComparison.Ordinal);
}
