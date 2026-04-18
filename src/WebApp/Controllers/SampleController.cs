using Microsoft.AspNetCore.Mvc;
using Shared.Dto;
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
    /// <returns>SSE レスポンスを書き込んだ後の空の結果。</returns>
    [HttpGet("progress")]
    [Produces("text/event-stream")]
    public async Task<EmptyResult> Progress(CancellationToken cancellationToken)
    {
        var context = new SampleTaskContext
        {
            TotalSteps = 10,
            CurrentStep = 0,
        };

        await SseUtility.StreamTaskProgressAsync(
            Response,
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
                        new SampleTaskProgressDto(
                            step,
                            taskContext.TotalSteps,
                            step * 100 / taskContext.TotalSteps,
                            $"{step}/{taskContext.TotalSteps} を処理中です。"));
                }
            },
            completedPayloadFactory: taskContext => new SampleTaskCompletedDto(
                "completed",
                taskContext.TotalSteps,
                taskContext.CurrentStep,
                DateTimeOffset.UtcNow),
            heartbeatInterval: TimeSpan.FromSeconds(5),
            cancellationToken: cancellationToken);

        return new EmptyResult();
    }
}
