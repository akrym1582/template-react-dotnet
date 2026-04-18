namespace Shared.Dto;

/// <summary>
/// サンプル SSE タスクの完了イベントデータ。
/// </summary>
/// <param name="Status">処理状態。</param>
/// <param name="TotalSteps">総ステップ数。</param>
/// <param name="ProcessedSteps">完了済みステップ数。</param>
/// <param name="Timestamp">完了時刻。</param>
public record SampleTaskCompletedDto(
    string Status,
    int TotalSteps,
    int ProcessedSteps,
    DateTimeOffset Timestamp);
