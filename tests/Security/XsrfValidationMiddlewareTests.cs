using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using WebApp.Security;

namespace Tests.Security;

public class XsrfValidationMiddlewareTests
{
    private readonly IAntiforgery _antiforgery;

    public XsrfValidationMiddlewareTests()
    {
        _antiforgery = Substitute.For<IAntiforgery>();
    }

    [Fact]
    public async Task 認証済みAPIリクエストではXSRF検証を行う()
    {
        var context = CreateContext("/api/auth/me", isAuthenticated: true);
        var nextCalled = false;
        var middleware = new XsrfValidationMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, _antiforgery);

        await _antiforgery.Received(1).ValidateRequestAsync(context);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task XSRF検証に失敗した場合は400を返す()
    {
        var context = CreateContext("/api/user", isAuthenticated: true);
        context.Response.Body = new MemoryStream();
        var middleware = new XsrfValidationMiddleware(_ => Task.CompletedTask);

        _antiforgery
            .ValidateRequestAsync(context)
            .Returns(_ => throw new AntiforgeryValidationException("invalid"));

        await middleware.InvokeAsync(context, _antiforgery);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task ログインAPIではXSRF検証をスキップする()
    {
        var context = CreateContext("/api/auth/login", isAuthenticated: true);
        var middleware = new XsrfValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, _antiforgery);

        await _antiforgery.DidNotReceive().ValidateRequestAsync(context);
    }

    [Fact]
    public async Task 未認証リクエストではXSRF検証をスキップする()
    {
        var context = CreateContext("/api/user", isAuthenticated: false);
        var middleware = new XsrfValidationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context, _antiforgery);

        await _antiforgery.DidNotReceive().ValidateRequestAsync(context);
    }

    private static DefaultHttpContext CreateContext(string path, bool isAuthenticated)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.User = isAuthenticated
            ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user-1")], "Test"))
            : new ClaimsPrincipal(new ClaimsIdentity());
        return context;
    }
}
