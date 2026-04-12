namespace Shared.Dto;

/// <summary>
/// ユーザー情報を表す DTO。
/// </summary>
/// <param name="UserId">ユーザーの一意識別子（GUID 文字列）。</param>
/// <param name="Email">メールアドレス。</param>
/// <param name="DisplayName">表示名。</param>
/// <param name="StoreCode">店舗コード。</param>
/// <param name="StoreName">店舗名。</param>
/// <param name="Roles">ロールの一覧。</param>
/// <param name="IsActive">アカウントが有効かどうか。</param>
/// <param name="MustChangePassword">次回ログイン時にパスワード変更が必要かどうか。</param>
public record UserDto(
    string UserId,
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool MustChangePassword);
