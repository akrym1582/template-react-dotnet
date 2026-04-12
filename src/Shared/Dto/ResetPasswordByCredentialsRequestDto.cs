namespace Shared.Dto;

/// <summary>
/// 現在のパスワードを使ったパスワードリセットリクエスト。
/// </summary>
/// <param name="Email">メールアドレス。</param>
/// <param name="CurrentPassword">現在のパスワード（認証用）。</param>
public record ResetPasswordByCredentialsRequestDto(string Email, string CurrentPassword);
