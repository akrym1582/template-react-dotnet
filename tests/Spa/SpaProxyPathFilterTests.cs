using WebApp.Spa;

namespace Tests.Spa;

public class SpaProxyPathFilterTests
{
    [Theory]
    [InlineData("/", true)]
    [InlineData("/login", true)]
    [InlineData("/api/auth/me", false)]
    [InlineData("/api/openapi/v1.json", false)]
    public void API配下のみSPAプロキシ対象から除外する(string path, bool expected)
    {
        Assert.Equal(expected, SpaProxyPathFilter.ShouldProxy(path));
    }
}
