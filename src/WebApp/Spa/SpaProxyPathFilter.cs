using Microsoft.AspNetCore.Http;

namespace WebApp.Spa;

public static class SpaProxyPathFilter
{
    public static bool ShouldProxy(PathString path)
    {
        return !path.StartsWithSegments("/api");
    }
}
