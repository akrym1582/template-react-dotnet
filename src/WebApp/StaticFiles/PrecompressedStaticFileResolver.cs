using Microsoft.AspNetCore.StaticFiles;

namespace WebApp.StaticFiles;

/// <summary>
/// クライアントの <c>Accept-Encoding</c> に応じて配信可能な事前圧縮済み静的ファイルを解決する。
/// </summary>
public class PrecompressedStaticFileResolver
{
    private const string BrotliEncoding = "br";
    private const string GzipEncoding = "gzip";
    private const string GzipExtension = ".gz";
    private const string BrotliExtension = ".br";

    private readonly string _rootPath;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;

    /// <summary>
    /// <see cref="PrecompressedStaticFileResolver"/> の新しいインスタンスを初期化する。
    /// </summary>
    /// <param name="rootPath">静的ファイルのルートディレクトリ。</param>
    public PrecompressedStaticFileResolver(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        _contentTypeProvider = new FileExtensionContentTypeProvider();
    }

    /// <summary>
    /// リクエストパスと <c>Accept-Encoding</c> から事前圧縮済みファイルを解決する。
    /// </summary>
    /// <param name="requestPath">要求された仮想パス。</param>
    /// <param name="acceptEncoding">リクエストヘッダーの <c>Accept-Encoding</c>。</param>
    /// <param name="file">解決された圧縮済みファイル情報。</param>
    /// <returns>圧縮済みファイルを配信できる場合は <see langword="true"/>。</returns>
    public bool TryResolve(PathString requestPath, string? acceptEncoding, out PrecompressedStaticFile? file)
    {
        file = null;

        var normalizedPath = NormalizeRequestPath(requestPath);
        if (normalizedPath is null)
        {
            return false;
        }

        var originalPath = GetFullPath(normalizedPath);
        if (originalPath is null || !File.Exists(originalPath))
        {
            return false;
        }

        var preferredEncoding = GetPreferredEncoding(acceptEncoding);
        if (preferredEncoding is null)
        {
            return false;
        }

        var compressedPath = $"{originalPath}{GetExtension(preferredEncoding)}";
        if (!File.Exists(compressedPath))
        {
            return false;
        }

        if (!_contentTypeProvider.TryGetContentType(requestPath.Value!, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        file = new PrecompressedStaticFile(
            compressedPath,
            contentType,
            preferredEncoding,
            File.GetLastWriteTimeUtc(compressedPath));

        return true;
    }

    private static string? NormalizeRequestPath(PathString requestPath)
    {
        if (!requestPath.HasValue || string.IsNullOrWhiteSpace(requestPath.Value))
        {
            return null;
        }

        var value = requestPath.Value!;
        if (value.EndsWith("/", StringComparison.Ordinal))
        {
            return null;
        }

        return value.TrimStart('/');
    }

    private static string? GetPreferredEncoding(string? acceptEncoding)
    {
        var brotliQuality = GetQuality(acceptEncoding, BrotliEncoding);
        var gzipQuality = GetQuality(acceptEncoding, GzipEncoding);

        if (brotliQuality <= 0 && gzipQuality <= 0)
        {
            return null;
        }

        return brotliQuality >= gzipQuality ? BrotliEncoding : GzipEncoding;
    }

    private static double GetQuality(string? acceptEncoding, string encoding)
    {
        if (string.IsNullOrWhiteSpace(acceptEncoding))
        {
            return 0;
        }

        double? wildcardQuality = null;

        foreach (var part in acceptEncoding.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var segments = part.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                continue;
            }

            var token = segments[0];
            var quality = 1D;

            foreach (var parameter in segments.Skip(1))
            {
                if (!parameter.StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!double.TryParse(
                    parameter.AsSpan(2),
                    System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out quality))
                {
                    quality = 0;
                }

                break;
            }

            if (token.Equals(encoding, StringComparison.OrdinalIgnoreCase))
            {
                return quality;
            }

            if (token.Equals("*", StringComparison.Ordinal))
            {
                wildcardQuality = quality;
            }
        }

        return wildcardQuality ?? 0;
    }

    private static string GetExtension(string encoding) =>
        encoding switch
        {
            BrotliEncoding => BrotliExtension,
            GzipEncoding => GzipExtension,
            _ => throw new InvalidOperationException("未対応の圧縮方式です。"),
        };

    private string? GetFullPath(string normalizedPath)
    {
        var combinedPath = Path.Combine(
            _rootPath,
            normalizedPath.Replace('/', Path.DirectorySeparatorChar));
        var fullPath = Path.GetFullPath(combinedPath);

        return IsUnderRoot(fullPath) ? fullPath : null;
    }

    private bool IsUnderRoot(string path) =>
        path.StartsWith($"{_rootPath}{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
        || path.Equals(_rootPath, StringComparison.Ordinal);
}
