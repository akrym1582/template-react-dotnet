namespace Shared.Dto;

/// <summary>
/// API レスポンスの共通フォーマット（データなし）。
/// </summary>
/// <param name="Success">処理が成功したかどうか。</param>
/// <param name="Message">補足メッセージ。</param>
public record ApiResponseDto(bool Success, string? Message = null);
