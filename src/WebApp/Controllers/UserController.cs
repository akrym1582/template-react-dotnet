using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto;
using Shared.Services;
using Shared.Util;

namespace WebApp.Controllers;

/// <summary>
/// ユーザー管理に関する API エンドポイントを提供するコントローラー。
/// ユーザーの取得・作成・更新・削除を担う。認証が必須。
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManagementSettings _settings;

    /// <summary>
    /// <see cref="UserController"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="userService">ユーザーサービス。</param>
    /// <param name="settings">ユーザー管理設定。</param>
    public UserController(IUserService userService, UserManagementSettings settings)
    {
        _userService = userService;
        _settings = settings;
    }

    /// <summary>
    /// 全ユーザーの一覧を取得する。マネージャー以上のロールが必要。
    /// </summary>
    /// <returns>ユーザー一覧。権限がない場合は 403 を返す。</returns>
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

    /// <summary>
    /// 指定した ID のユーザー情報を取得する。
    /// 自分自身またはマネージャー以上のロールが必要。
    /// </summary>
    /// <param name="userId">取得するユーザーの ID。</param>
    /// <returns>ユーザー情報。権限がない場合は 403、見つからない場合は 404 を返す。</returns>
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

    /// <summary>
    /// 新しいユーザーを作成する。マネージャー以上のロールが必要。
    /// また、設定でマネージャーによるユーザー作成が許可されている必要がある。
    /// </summary>
    /// <param name="request">ユーザー作成リクエスト。</param>
    /// <returns>作成されたユーザー情報と初期パスワード。権限がない場合は 403、メール重複時は 400 を返す。</returns>
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

    /// <summary>
    /// 指定した ID のユーザー情報を更新する。
    /// 自分自身またはマネージャー以上のロールが必要。ロール変更はマネージャー以上のみ可能。
    /// </summary>
    /// <param name="userId">更新するユーザーの ID。</param>
    /// <param name="request">ユーザー更新リクエスト。</param>
    /// <returns>更新後のユーザー情報。権限がない場合は 403、見つからない場合は 404 を返す。</returns>
    [HttpPut("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> Update(string userId, [FromBody] UpdateUserRequestDto request)
    {
        var isSelf = IsCurrentUser(userId);
        var canManageUsers = RoleHelper.CanManageUsers(User);

        if (!isSelf && !canManageUsers)
            return Forbid();

        if (!canManageUsers && request.Roles is not null)
            return Forbid();

        var existingUser = await _userService.GetByEmailAsync(request.Email);
        if (existingUser is not null && !string.Equals(existingUser.UserId, userId, StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// 指定した ID のユーザーを削除する。マネージャー以上のロールが必要。
    /// </summary>
    /// <param name="userId">削除するユーザーの ID。</param>
    /// <returns>削除成功メッセージ。権限がない場合は 403 を返す。</returns>
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
