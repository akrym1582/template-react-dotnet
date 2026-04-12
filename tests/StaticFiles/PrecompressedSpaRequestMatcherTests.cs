using Microsoft.AspNetCore.Http;
using WebApp.StaticFiles;

namespace Tests.StaticFiles;

public class PrecompressedSpaRequestMatcherTests
{
    [Fact]
    public void 静的ファイルへのGetリクエストは探索対象にする()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/assets/app.js";

        var result = PrecompressedSpaRequestMatcher.ShouldTryResolvePrecompressedAsset(httpContext.Request);

        Assert.True(result);
    }

    [Fact]
    public void Api配下の拡張子付きGetリクエストは探索対象にしない()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/api/openapi/v1.json";

        var result = PrecompressedSpaRequestMatcher.ShouldTryResolvePrecompressedAsset(httpContext.Request);

        Assert.False(result);
    }

    [Fact]
    public void Api配下のパスはApiリクエストとして判定する()
    {
        var result = PrecompressedSpaRequestMatcher.IsApiRequest("/api/users");

        Assert.True(result);
    }
}
