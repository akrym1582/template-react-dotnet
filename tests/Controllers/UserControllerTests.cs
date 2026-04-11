using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shared.Dto;
using Shared.Services;
using Shared.Util;
using WebApp.Controllers;

namespace Tests.Controllers;

public class UserControllerTests
{
    private readonly IUserService _userService;

    public UserControllerTests()
    {
        _userService = Substitute.For<IUserService>();
    }

    [Fact]
    public async Task GetById_一般ユーザーが他人を参照するとForbidになる()
    {
        var controller = CreateController("self-user", [Constants.Roles.General]);

        var result = await controller.GetById("other-user");

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Update_一般ユーザーが自分のロール変更を試みるとForbidになる()
    {
        _userService.GetByEmailAsync("self@example.com").Returns((UserDto?)null);
        _userService.GetByIdAsync("self-user").Returns(new UserDto(
            "self-user",
            "self@example.com",
            "自分",
            "001",
            "本店",
            [Constants.Roles.General],
            true,
            false));

        var controller = CreateController("self-user", [Constants.Roles.General]);

        var result = await controller.Update(
            "self-user",
            new UpdateUserRequestDto("self@example.com", "自分", "001", "本店", [Constants.Roles.Manager]));

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Create_作成設定が無効な場合はForbidになる()
    {
        var controller = CreateController("manager-user", [Constants.Roles.Manager], allowUserCreation: false);

        var result = await controller.Create(
            new CreateUserRequestDto("new@example.com", "新規", "001", "本店", [Constants.Roles.General]));

        Assert.IsType<ForbidResult>(result.Result);
    }

    private UserController CreateController(
        string userId,
        IReadOnlyList<string> roles,
        bool allowUserCreation = true)
    {
        var controller = new UserController(
            _userService,
            new UserManagementSettings
            {
                AllowManagerUserCreation = allowUserCreation,
                InitialPassword = "Init@1234"
            });

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        .. roles.Select(role => new Claim(ClaimTypes.Role, role))
                    ],
                    "Test"))
            }
        };

        return controller;
    }
}
