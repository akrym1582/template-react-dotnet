using Microsoft.AspNetCore.Http;

namespace WebApp.Sse;

/// <summary>
/// 長時間タスクの進捗を SSE で返却するユーティリティ。
/// </summary>
public static class SseUtility
{
    /// <summary>
    /// 長時間タスクの進捗を SSE で返却する。
    /// </summary>
    /// <typeparam name="TContext">進捗管理に使用するコンテキストの型。</typeparam>
    /// <param name="response">SSE を返す HTTP レスポンス。</param>
    /// <param name="context">進捗管理用コンテキスト。</param>
    /// <param name="taskBody">長時間処理本体。</param>
    /// <param name="completedPayloadFactory">完了イベントのペイロード生成処理。</param>
    /// <param name="errorPayloadFactory">エラーイベントのペイロード生成処理。</param>
    /// <param name="heartbeatInterval">heartbeat の送信間隔。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>非同期処理を表すタスク。</returns>
    public static async Task StreamTaskProgressAsync<TContext>(
        HttpResponse response,
        TContext context,
        Func<TContext, Func<string, object, Task>, CancellationToken, Task> taskBody,
        Func<TContext, object>? completedPayloadFactory = null,
        Func<Exception, object>? errorPayloadFactory = null,
        TimeSpan? heartbeatInterval = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(taskBody);

        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "text/event-stream; charset=utf-8";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["X-Accel-Buffering"] = "no";

        using var internalCancellationTokenSource = new CancellationTokenSource();
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            internalCancellationTokenSource.Token);
        using var heartbeatCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            linkedCancellationTokenSource.Token);
        using var writeLock = new SemaphoreSlim(1, 1);

        var linkedCancellationToken = linkedCancellationTokenSource.Token;
        Task? heartbeatTask = null;

        try
        {
            await WriteEventAsync(
                "started",
                CreateStartedPayload(),
                linkedCancellationToken);

            if (heartbeatInterval is { } interval && interval > TimeSpan.Zero)
            {
                heartbeatTask = RunHeartbeatAsync(interval, heartbeatCancellationTokenSource.Token);
            }

            await taskBody(context, ReportAsync, linkedCancellationToken);

            heartbeatCancellationTokenSource.Cancel();
            await AwaitHeartbeatAsync(heartbeatTask);

            if (!IsCancellationRequested(response, cancellationToken))
            {
                await WriteEventAsync(
                    "completed",
                    completedPayloadFactory?.Invoke(context) ?? CreateCompletedPayload(),
                    linkedCancellationToken);
            }
        }
        catch (OperationCanceledException) when (IsCancellationRequested(response, cancellationToken) || linkedCancellationToken.IsCancellationRequested)
        {
        }
        catch (IOException) when (IsCancellationRequested(response, cancellationToken))
        {
        }
        catch (Exception exception)
        {
            heartbeatCancellationTokenSource.Cancel();
            await AwaitHeartbeatAsync(heartbeatTask);

            if (!IsCancellationRequested(response, cancellationToken))
            {
                try
                {
                    await WriteEventAsync(
                        "error",
                        errorPayloadFactory?.Invoke(exception) ?? CreateErrorPayload(exception),
                        linkedCancellationToken);
                }
                catch (OperationCanceledException) when (IsCancellationRequested(response, cancellationToken))
                {
                }
                catch (IOException) when (IsCancellationRequested(response, cancellationToken))
                {
                }
            }
        }
        finally
        {
            heartbeatCancellationTokenSource.Cancel();
            await AwaitHeartbeatAsync(heartbeatTask);
            internalCancellationTokenSource.Cancel();
        }

        async Task ReportAsync(string eventName, object payload) =>
            await WriteEventAsync(eventName, payload, linkedCancellationToken);

        async Task WriteEventAsync(string eventName, object payload, CancellationToken writeCancellationToken)
        {
            var lockTaken = false;

            try
            {
                await writeLock.WaitAsync(writeCancellationToken);
                lockTaken = true;
                await SseWriter.WriteEventAsync(response, eventName, payload, writeCancellationToken);
            }
            finally
            {
                if (lockTaken)
                {
                    writeLock.Release();
                }
            }
        }

        async Task RunHeartbeatAsync(TimeSpan interval, CancellationToken heartbeatCancellationToken)
        {
            while (!heartbeatCancellationToken.IsCancellationRequested)
            {
                var lockTaken = false;

                try
                {
                    await Task.Delay(interval, heartbeatCancellationToken);
                    await writeLock.WaitAsync(heartbeatCancellationToken);
                    lockTaken = true;
                    await SseWriter.WriteCommentAsync(response, "heartbeat", heartbeatCancellationToken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        writeLock.Release();
                    }
                }
            }
        }
    }

    private static async Task AwaitHeartbeatAsync(Task? heartbeatTask)
    {
        if (heartbeatTask is null)
        {
            return;
        }

        try
        {
            await heartbeatTask;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static bool IsCancellationRequested(HttpResponse response, CancellationToken cancellationToken) =>
        cancellationToken.IsCancellationRequested || response.HttpContext.RequestAborted.IsCancellationRequested;

    private static object CreateStartedPayload() =>
        new
        {
            status = "started",
            timestamp = DateTimeOffset.UtcNow,
        };

    private static object CreateCompletedPayload() =>
        new
        {
            status = "completed",
            timestamp = DateTimeOffset.UtcNow,
        };

    private static object CreateErrorPayload(Exception exception) =>
        new
        {
            status = "error",
            message = exception.Message,
        };
}
