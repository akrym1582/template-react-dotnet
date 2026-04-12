namespace Shared.Dto;

/// <summary>
/// ユーザー作成結果を表す DTO。
/// </summary>
/// <param name="User">作成されたユーザーの情報。</param>
/// <param name="InitialPassword">初期パスワード。</param>
/// <param name="MustChangePassword">次回ログイン時にパスワード変更が必要かどうか。</param>
public record CreatedUserResultDto(
    UserDto User,
    string InitialPassword,
    bool MustChangePassword);
