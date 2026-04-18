using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers;
using WebApp.Sse;

namespace Tests.Sse;

public class SseUtilityTests
{
    [Fact]
    public async Task StreamTaskProgressAsync_正常終了時は開始進捗完了イベントを返す()
    {
        var context = new SampleTaskContext
        {
            TotalSteps = 2,
            CurrentStep = 0,
        };
        var httpContext = CreateHttpContext();

        await SseUtility.StreamTaskProgressAsync(
            httpContext.Response,
            context,
            async (taskContext, report, _) =>
            {
                taskContext.CurrentStep = 1;
                await report("progress", new { percent = 50, message = "processing" });
                await report("notice", new { step = taskContext.CurrentStep });
                taskContext.CurrentStep = taskContext.TotalSteps;
            },
            completedPayloadFactory: taskContext => new
            {
                status = "completed",
                processedSteps = taskContext.CurrentStep,
            });

        var payload = ReadResponseBody(httpContext.Response);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal("text/event-stream; charset=utf-8", httpContext.Response.ContentType);
        Assert.Equal("no-cache", httpContext.Response.Headers["Cache-Control"].ToString());
        Assert.Equal("no", httpContext.Response.Headers["X-Accel-Buffering"].ToString());
        Assert.True(payload.IndexOf("event: started\n", StringComparison.Ordinal) >= 0);
        Assert.True(payload.IndexOf("event: progress\n", StringComparison.Ordinal) > payload.IndexOf("event: started\n", StringComparison.Ordinal));
        Assert.True(payload.IndexOf("event: completed\n", StringComparison.Ordinal) > payload.IndexOf("event: progress\n", StringComparison.Ordinal));
        Assert.Contains("event: notice\n", payload, StringComparison.Ordinal);
        Assert.Contains("data: {\"percent\":50,\"message\":\"processing\"}\n\n", payload, StringComparison.Ordinal);
        Assert.Contains("\"processedSteps\":2", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StreamTaskProgressAsync_heartbeat指定時はコメントを返す()
    {
        var httpContext = CreateHttpContext();

        await SseUtility.StreamTaskProgressAsync(
            httpContext.Response,
            new object(),
            async (_, _, cancellationToken) => await Task.Delay(TimeSpan.FromMilliseconds(70), cancellationToken),
            heartbeatInterval: TimeSpan.FromMilliseconds(20));

        var payload = ReadResponseBody(httpContext.Response);

        Assert.Contains(": heartbeat\n\n", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StreamTaskProgressAsync_例外発生時はerrorイベントを返して再送出しない()
    {
        var httpContext = CreateHttpContext();

        var exception = await Record.ExceptionAsync(async () =>
            await SseUtility.StreamTaskProgressAsync(
                httpContext.Response,
                new object(),
                (_, _, _) => throw new InvalidOperationException("boom")));

        var payload = ReadResponseBody(httpContext.Response);

        Assert.Null(exception);
        Assert.Contains("event: error\n", payload, StringComparison.Ordinal);
        Assert.Contains("\"status\":\"error\"", payload, StringComparison.Ordinal);
        Assert.Contains("\"message\":\"boom\"", payload, StringComparison.Ordinal);
        Assert.DoesNotContain("event: completed\n", payload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StreamTaskProgressAsync_SampleControllerで使用時にSseレスポンスを返す()
    {
        var httpContext = CreateHttpContext();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(700));

        var controller = new SampleController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },
        };

        var result = await controller.Progress(cancellationTokenSource.Token);

        var payload = ReadResponseBody(httpContext.Response);

        Assert.IsType<EmptyResult>(result);
        Assert.Equal("text/event-stream; charset=utf-8", httpContext.Response.ContentType);
        Assert.Contains("event: started\n", payload, StringComparison.Ordinal);
        Assert.Contains("event: progress\n", payload, StringComparison.Ordinal);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static string ReadResponseBody(HttpResponse response)
    {
        response.Body.Position = 0;
        return Encoding.UTF8.GetString(((MemoryStream)response.Body).ToArray());
    }
}
