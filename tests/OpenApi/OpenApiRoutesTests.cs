using WebApp.OpenApi;

namespace Tests.OpenApi;

public class OpenApiRoutesTests
{
    [Fact]
    public void OpenApi仕様のURLはAPI配下を使う()
    {
        Assert.Equal("/api/openapi/{documentName}.json", OpenApiRoutes.ApiDocumentPath);
    }
}
