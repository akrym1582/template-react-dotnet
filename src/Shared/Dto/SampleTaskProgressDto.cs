namespace Shared.Dto;

/// <summary>
/// サンプル SSE タスクの進捗イベントデータ。
/// </summary>
/// <param name="Step">現在のステップ番号。</param>
/// <param name="TotalSteps">総ステップ数。</param>
/// <param name="Percent">進捗率（0 から 100）。</param>
/// <param name="Message">進捗メッセージ。</param>
public record SampleTaskProgressDto(
    int Step,
    int TotalSteps,
    int Percent,
    string Message);
