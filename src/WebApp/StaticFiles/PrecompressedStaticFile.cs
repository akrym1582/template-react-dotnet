namespace WebApp.StaticFiles;

/// <summary>
/// 事前圧縮済み静的ファイルの配信情報を表す。
/// </summary>
/// <param name="PhysicalPath">配信する圧縮済みファイルの物理パス。</param>
/// <param name="ContentType">元ファイルに対応する Content-Type。</param>
/// <param name="ContentEncoding">レスポンスに設定する Content-Encoding。</param>
/// <param name="LastModified">圧縮済みファイルの最終更新日時。</param>
public sealed record PrecompressedStaticFile(
    string PhysicalPath,
    string ContentType,
    string ContentEncoding,
    DateTimeOffset LastModified);
