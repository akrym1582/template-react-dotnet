namespace Shared.Dto;

/// <summary>
/// ユーザー情報更新リクエスト。
/// </summary>
/// <param name="Email">メールアドレス。</param>
/// <param name="DisplayName">表示名。</param>
/// <param name="StoreCode">店舗コード。</param>
/// <param name="StoreName">店舗名。</param>
/// <param name="Roles">更新するロールの一覧。<c>null</c> の場合はロールを変更しない。</param>
public record UpdateUserRequestDto(
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string>? Roles);
