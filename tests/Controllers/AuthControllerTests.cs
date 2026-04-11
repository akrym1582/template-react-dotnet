using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shared.Dto;
using Shared.Services;
using WebApp.Controllers;
using WebApp.Options;

namespace Tests.Controllers;

public class AuthControllerTests
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authenticationService;

    public AuthControllerTests()
    {
        _userService = Substitute.For<IUserService>();
        _authenticationService = Substitute.For<IAuthenticationService>();
    }

    [Fact]
    public async Task TestLogin_設定済みユーザーの場合はログインできる()
    {
        var controller = CreateController(
            new TestLoginOptions
            {
                Users =
                [
                    new TestLoginUserOption
                    {
                        UserId = "test-admin",
                        Roles = ["admin", "user"]
                    }
                ]
            });

        var result = await controller.TestLogin(new TestLoginRequestDto("test-admin"));

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<UserDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("test-admin", response.Data.UserId);
        Assert.Equal(["admin", "user"], response.Data.Roles);

        await _authenticationService.Received(1).SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Any<string?>(),
            Arg.Is<ClaimsPrincipal>(principal =>
                principal.HasClaim(ClaimTypes.NameIdentifier, "test-admin")
                && principal.IsInRole("admin")
                && principal.IsInRole("user")),
            Arg.Any<AuthenticationProperties?>());
    }

    [Fact]
    public async Task TestLogin_未設定ユーザーの場合はUnauthorizedを返す()
    {
        var controller = CreateController(new TestLoginOptions());

        var result = await controller.TestLogin(new TestLoginRequestDto("unknown-user"));

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto>(unauthorizedResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Me_テストログインユーザーの場合は設定値からユーザー情報を返す()
    {
        _userService.GetByIdAsync("test-user").Returns((UserDto?)null);

        var controller = CreateController(
            new TestLoginOptions
            {
                Users =
                [
                    new TestLoginUserOption
                    {
                        UserId = "test-user",
                        Roles = ["user"]
                    }
                ]
            });

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            ],
            "Test"));

        var result = await controller.Me();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponseDto<UserDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("test-user", response.Data.UserId);
        Assert.Equal("test-user@test.local", response.Data.Email);
        Assert.Equal(["user"], response.Data.Roles);
    }

    private AuthController CreateController(TestLoginOptions testLoginOptions)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns("Development");
        environment.ApplicationName.Returns("WebApp");
        environment.ContentRootPath.Returns("/");
        environment.ContentRootFileProvider.Returns(Substitute.For<IFileProvider>());
        environment.WebRootPath.Returns("/");
        environment.WebRootFileProvider.Returns(Substitute.For<IFileProvider>());

        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(_authenticationService)
                .BuildServiceProvider()
        };

        return new AuthController(
            new Lazy<IUserService>(() => _userService),
            Options.Create(testLoginOptions),
            environment)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}
