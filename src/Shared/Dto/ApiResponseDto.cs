namespace Shared.Dto;

/// <summary>
/// API レスポンスの共通フォーマット（データあり）。
/// </summary>
/// <typeparam name="T">レスポンスデータの型。</typeparam>
/// <param name="Success">処理が成功したかどうか。</param>
/// <param name="Data">レスポンスデータ。</param>
/// <param name="Message">補足メッセージ。</param>
public record ApiResponseDto<T>(bool Success, T? Data = default, string? Message = null);
