using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dto;
using Shared.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<UserDto>>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(new ApiResponseDto<IReadOnlyList<UserDto>>(true, users));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetById(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new ApiResponseDto(false, "ユーザーが見つかりません。"));

        return Ok(new ApiResponseDto<UserDto>(true, user));
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponseDto>> Delete(string userId)
    {
        await _userService.DeleteAsync(userId);
        return Ok(new ApiResponseDto(true, "削除しました。"));
    }
}
