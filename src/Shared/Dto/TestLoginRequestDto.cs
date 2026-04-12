namespace Shared.Dto;

/// <summary>
/// テストログインリクエスト（開発環境専用）。
/// </summary>
/// <param name="UserId">ログインするテストユーザーの ID。</param>
public record TestLoginRequestDto(string UserId);
