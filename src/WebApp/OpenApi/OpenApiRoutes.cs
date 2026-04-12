namespace WebApp.OpenApi;

/// <summary>
/// OpenAPI ドキュメントのルート定数を定義する静的クラス。
/// </summary>
public static class OpenApiRoutes
{
    /// <summary>OpenAPI ドキュメントの URL パス。{documentName} はドキュメント名に置換される。</summary>
    public const string ApiDocumentPath = "/api/openapi/{documentName}.json";
}
