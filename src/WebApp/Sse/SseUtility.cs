using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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
    /// <param name="context">進捗管理用コンテキスト。</param>
    /// <param name="taskBody">長時間処理本体。</param>
    /// <param name="completedPayloadFactory">完了イベントのペイロード生成処理。</param>
    /// <param name="errorPayloadFactory">エラーイベントのペイロード生成処理。</param>
    /// <param name="heartbeatInterval">heartbeat の送信間隔。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>SSE レスポンスを返す結果。</returns>
    public static ServerSentEventsResult<object> StreamTaskProgress<TContext>(
        TContext context,
        Func<TContext, Func<string, object, Task>, CancellationToken, Task> taskBody,
        Func<TContext, object>? completedPayloadFactory = null,
        Func<Exception, object>? errorPayloadFactory = null,
        TimeSpan? heartbeatInterval = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskBody);

        var values = CreateEventStream(
            context,
            taskBody,
            completedPayloadFactory,
            errorPayloadFactory,
            heartbeatInterval,
            cancellationToken);

        return TypedResults.ServerSentEvents(values);
    }

    private static async IAsyncEnumerable<SseItem<object>> CreateEventStream<TContext>(
        TContext context,
        Func<TContext, Func<string, object, Task>, CancellationToken, Task> taskBody,
        Func<TContext, object>? completedPayloadFactory,
        Func<Exception, object>? errorPayloadFactory,
        TimeSpan? heartbeatInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var internalCancellationTokenSource = new CancellationTokenSource();
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            internalCancellationTokenSource.Token);
        using var heartbeatCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            linkedCancellationTokenSource.Token);

        var channel = Channel.CreateUnbounded<SseItem<object>>();
        var linkedCancellationToken = linkedCancellationTokenSource.Token;
        Task? heartbeatTask = null;
        var producerTask = Task.Run(
            async () =>
            {
                try
                {
                    if (heartbeatInterval is { } interval && interval > TimeSpan.Zero)
                    {
                        heartbeatTask = RunHeartbeatAsync(interval, heartbeatCancellationTokenSource.Token);
                    }

                    await taskBody(context, ReportAsync, linkedCancellationToken);

                    heartbeatCancellationTokenSource.Cancel();
                    await AwaitHeartbeatAsync(heartbeatTask);

                    if (!linkedCancellationToken.IsCancellationRequested)
                    {
                        await channel.Writer.WriteAsync(
                            new SseItem<object>(
                                completedPayloadFactory?.Invoke(context) ?? CreateCompletedPayload(),
                                "completed"),
                            linkedCancellationToken);
                    }
                }
                catch (OperationCanceledException) when (linkedCancellationToken.IsCancellationRequested)
                {
                }
                catch (Exception exception)
                {
                    heartbeatCancellationTokenSource.Cancel();
                    await AwaitHeartbeatAsync(heartbeatTask);

                    if (!linkedCancellationToken.IsCancellationRequested)
                    {
                        await channel.Writer.WriteAsync(
                            new SseItem<object>(
                                errorPayloadFactory?.Invoke(exception) ?? CreateErrorPayload(exception),
                                "error"),
                            CancellationToken.None);
                    }
                }
                finally
                {
                    heartbeatCancellationTokenSource.Cancel();
                    await AwaitHeartbeatAsync(heartbeatTask);
                    internalCancellationTokenSource.Cancel();
                    channel.Writer.TryComplete();
                }
            },
            CancellationToken.None);

        yield return new SseItem<object>(CreateStartedPayload(), "started");

        await using var eventEnumerator = channel.Reader.ReadAllAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);
        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = await eventEnumerator.MoveNextAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!hasNext)
            {
                break;
            }

            yield return eventEnumerator.Current;
        }

        try
        {
            await producerTask;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }

        async Task ReportAsync(string eventName, object payload)
        {
            if (linkedCancellationToken.IsCancellationRequested)
            {
                return;
            }

            await channel.Writer.WriteAsync(new SseItem<object>(payload, eventName), linkedCancellationToken);
        }

        async Task RunHeartbeatAsync(TimeSpan interval, CancellationToken heartbeatCancellationToken)
        {
            while (!heartbeatCancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, heartbeatCancellationToken);
                if (!heartbeatCancellationToken.IsCancellationRequested)
                {
                    await channel.Writer.WriteAsync(
                        new SseItem<object>(CreateHeartbeatPayload(), "heartbeat"),
                        heartbeatCancellationToken);
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

    private static object CreateStartedPayload() =>
        new
        {
            status = "started",
            timestamp = DateTimeOffset.UtcNow,
        };

    private static object CreateHeartbeatPayload() =>
        new
        {
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
