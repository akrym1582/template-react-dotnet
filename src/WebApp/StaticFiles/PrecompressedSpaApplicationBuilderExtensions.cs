using Microsoft.Extensions.FileProviders;

namespace WebApp.StaticFiles;

/// <summary>
/// production 用 SPA 静的ファイル配信を構成する拡張メソッド。
/// </summary>
public static class PrecompressedSpaApplicationBuilderExtensions
{
    private const string SpaIndexPath = "/index.html";
    private const string SpaIndexContentType = "text/html; charset=utf-8";

    /// <summary>
    /// 事前圧縮済み資材の配信を含む SPA 静的ファイル配信を構成する。
    /// </summary>
    /// <param name="app">対象の Web アプリケーション。</param>
    /// <returns>構成済みの <paramref name="app"/>。</returns>
    public static WebApplication UsePrecompressedSpaStaticFiles(this WebApplication app)
    {
        var spaDistPath = Path.Combine(app.Environment.ContentRootPath, "clientapp", "dist");
        var spaStaticFiles = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(spaDistPath),
        };
        var precompressedStaticFileResolver = new PrecompressedStaticFileResolver(spaDistPath);

        app.Use(async (context, next) =>
        {
            if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
                && Path.HasExtension(context.Request.Path.Value)
                && precompressedStaticFileResolver.TryResolve(
                    context.Request.Path,
                    context.Request.Headers.AcceptEncoding,
                    out var precompressedFile)
                && precompressedFile is not null)
            {
                ApplyPrecompressedResponseHeaders(context, precompressedFile);

                await Results.File(
                    precompressedFile.PhysicalPath,
                    precompressedFile.ContentType,
                    lastModified: precompressedFile.LastModified,
                    enableRangeProcessing: true).ExecuteAsync(context);

                return;
            }

            await next();
        });

        app.UseStaticFiles(spaStaticFiles);
        app.MapFallback(async context =>
        {
            if (precompressedStaticFileResolver.TryResolve(
                SpaIndexPath,
                context.Request.Headers.AcceptEncoding,
                out var precompressedFile)
                && precompressedFile is not null)
            {
                ApplyPrecompressedResponseHeaders(context, precompressedFile);

                await Results.File(
                    precompressedFile.PhysicalPath,
                    precompressedFile.ContentType,
                    lastModified: precompressedFile.LastModified,
                    enableRangeProcessing: true).ExecuteAsync(context);

                return;
            }

            var indexPath = Path.Combine(spaDistPath, "index.html");
            await Results.File(
                indexPath,
                SpaIndexContentType,
                lastModified: File.GetLastWriteTimeUtc(indexPath)).ExecuteAsync(context);
        });

        return app;
    }

    private static void ApplyPrecompressedResponseHeaders(HttpContext context, PrecompressedStaticFile file)
    {
        context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentEncoding] = file.ContentEncoding;
        context.Response.Headers.AppendCommaSeparatedValues(
            Microsoft.Net.Http.Headers.HeaderNames.Vary,
            Microsoft.Net.Http.Headers.HeaderNames.AcceptEncoding);
    }
}
