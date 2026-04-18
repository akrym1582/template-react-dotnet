using Microsoft.AspNetCore.Http;
using Shared.Util;

namespace WebApp.Sse;

/// <summary>
/// SSE のイベントおよびコメントを書き込むユーティリティ。
/// </summary>
public static class SseWriter
{
    /// <summary>
    /// SSE のイベントを書き込む。
    /// </summary>
    /// <param name="response">書き込み先の HTTP レスポンス。</param>
    /// <param name="eventName">イベント名。</param>
    /// <param name="data">JSON として返却するデータ。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>非同期処理を表すタスク。</returns>
    public static async Task WriteEventAsync(
        HttpResponse response,
        string eventName,
        object data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(data);

        await response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await response.WriteAsync($"data: {JsonHelper.Serialize(data)}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// SSE のコメントを書き込む。
    /// </summary>
    /// <param name="response">書き込み先の HTTP レスポンス。</param>
    /// <param name="comment">コメント文字列。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>非同期処理を表すタスク。</returns>
    public static async Task WriteCommentAsync(
        HttpResponse response,
        string comment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(comment);

        await response.WriteAsync($": {comment}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
