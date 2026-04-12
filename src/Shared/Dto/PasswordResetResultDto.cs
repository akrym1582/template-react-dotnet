namespace Shared.Dto;

/// <summary>
/// パスワードリセット結果を表す DTO。
/// </summary>
/// <param name="InitialPassword">リセット後の初期パスワード。</param>
/// <param name="MustChangePassword">次回ログイン時にパスワード変更が必要かどうか。</param>
public record PasswordResetResultDto(string InitialPassword, bool MustChangePassword);
