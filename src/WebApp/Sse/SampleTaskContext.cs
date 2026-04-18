namespace WebApp.Sse;

/// <summary>
/// サンプル SSE タスクの進捗情報を保持するコンテキスト。
/// </summary>
public sealed class SampleTaskContext
{
    /// <summary>
    /// 総ステップ数。
    /// </summary>
    public int TotalSteps { get; set; }

    /// <summary>
    /// 現在完了しているステップ数。
    /// </summary>
    public int CurrentStep { get; set; }
}
