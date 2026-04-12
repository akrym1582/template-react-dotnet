namespace WebApp.StaticFiles;

/// <summary>
/// 事前圧縮済み SPA 静的ファイル配信の対象リクエストかどうかを判定する。
/// </summary>
public static class PrecompressedSpaRequestMatcher
{
    /// <summary>
    /// 事前圧縮済み静的ファイルの探索対象かどうかを判定する。
    /// </summary>
    /// <param name="request">対象リクエスト。</param>
    /// <returns>探索対象の場合は <see langword="true"/>。</returns>
    public static bool ShouldTryResolvePrecompressedAsset(HttpRequest request) =>
        (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method))
        && Path.HasExtension(request.Path.Value)
        && !IsApiRequest(request.Path);

    /// <summary>
    /// API リクエストかどうかを判定する。
    /// </summary>
    /// <param name="path">対象パス。</param>
    /// <returns>API リクエストの場合は <see langword="true"/>。</returns>
    public static bool IsApiRequest(PathString path) =>
        path.Value is not null
        && (path.Value.Equals("/api", StringComparison.OrdinalIgnoreCase)
            || path.Value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase));
}
