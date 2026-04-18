using Microsoft.AspNetCore.Mvc;
using WebApp.Sse;

namespace WebApp.Controllers;

/// <summary>
/// SSE による進捗配信のサンプル API を提供するコントローラー。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    /// <summary>
    /// 仮の長時間タスク進捗を SSE で返却する。
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>SSE レスポンスを返す結果。</returns>
    [HttpGet("progress")]
    public IResult Progress(CancellationToken cancellationToken)
    {
        var context = new SampleTaskContext
        {
            TotalSteps = 10,
            CurrentStep = 0,
        };

        return SseUtility.StreamTaskProgress(
            context,
            taskBody: async (taskContext, report, token) =>
            {
                for (var step = 1; step <= taskContext.TotalSteps; step++)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromMilliseconds(600), token);

                    taskContext.CurrentStep = step;

                    await report(
                        "progress",
                        new
                        {
                            step,
                            totalSteps = taskContext.TotalSteps,
                            percent = step * 100 / taskContext.TotalSteps,
                            message = $"{step}/{taskContext.TotalSteps} を処理中です。",
                        });
                }
            },
            completedPayloadFactory: taskContext => new
            {
                status = "completed",
                totalSteps = taskContext.TotalSteps,
                processedSteps = taskContext.CurrentStep,
                timestamp = DateTimeOffset.UtcNow,
            },
            heartbeatInterval: TimeSpan.FromSeconds(5),
            cancellationToken: cancellationToken);
    }
}
