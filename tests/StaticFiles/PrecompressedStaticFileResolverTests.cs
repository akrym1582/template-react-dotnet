using WebApp.StaticFiles;

namespace Tests.StaticFiles;

public class PrecompressedStaticFileResolverTests
{
    [Fact]
    public void Brotliが利用可能な場合はBrotli圧縮ファイルを優先する()
    {
        using var testDirectory = new TestDirectory();
        testDirectory.CreateFile("assets/app.js", "console.log('app');");
        testDirectory.CreateFile("assets/app.js.br", "brotli");
        testDirectory.CreateFile("assets/app.js.gz", "gzip");

        var resolver = new PrecompressedStaticFileResolver(testDirectory.Path);

        var result = resolver.TryResolve("/assets/app.js", "gzip, br", out var file);

        Assert.True(result);
        Assert.NotNull(file);
        Assert.Equal("br", file.ContentEncoding);
        Assert.Equal("text/javascript", file.ContentType);
        Assert.EndsWith("assets/app.js.br", file.PhysicalPath, StringComparison.Ordinal);
    }

    [Fact]
    public void Gzipのみ利用可能な場合はgzip圧縮ファイルを返す()
    {
        using var testDirectory = new TestDirectory();
        testDirectory.CreateFile("assets/app.css", "body{}");
        testDirectory.CreateFile("assets/app.css.gz", "gzip");

        var resolver = new PrecompressedStaticFileResolver(testDirectory.Path);

        var result = resolver.TryResolve("/assets/app.css", "gzip", out var file);

        Assert.True(result);
        Assert.NotNull(file);
        Assert.Equal("gzip", file.ContentEncoding);
        Assert.Equal("text/css", file.ContentType);
        Assert.EndsWith("assets/app.css.gz", file.PhysicalPath, StringComparison.Ordinal);
    }

    [Fact]
    public void Q値が0の圧縮方式は選択しない()
    {
        using var testDirectory = new TestDirectory();
        testDirectory.CreateFile("index.html", "<html></html>");
        testDirectory.CreateFile("index.html.br", "brotli");
        testDirectory.CreateFile("index.html.gz", "gzip");

        var resolver = new PrecompressedStaticFileResolver(testDirectory.Path);

        var result = resolver.TryResolve("/index.html", "br;q=0, gzip;q=0", out var file);

        Assert.False(result);
        Assert.Null(file);
    }

    [Fact]
    public void ルート外を指すパスは解決しない()
    {
        using var testDirectory = new TestDirectory();
        testDirectory.CreateFile("index.html", "<html></html>");
        testDirectory.CreateFile("index.html.br", "brotli");

        var resolver = new PrecompressedStaticFileResolver(testDirectory.Path);

        var result = resolver.TryResolve("/../index.html", "br", out var file);

        Assert.False(result);
        Assert.Null(file);
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }

        public void CreateFile(string relativePath, string contents)
        {
            var fullPath = System.IO.Path.Combine(
                Path,
                relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            var directoryPath = System.IO.Path.GetDirectoryName(fullPath);

            if (directoryPath is not null)
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(fullPath, contents);
        }
    }
}
