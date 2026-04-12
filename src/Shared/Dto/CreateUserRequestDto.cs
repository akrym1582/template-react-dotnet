namespace Shared.Dto;

/// <summary>
/// ユーザー作成リクエスト。
/// </summary>
/// <param name="Email">メールアドレス。</param>
/// <param name="DisplayName">表示名。</param>
/// <param name="StoreCode">店舗コード。</param>
/// <param name="StoreName">店舗名。</param>
/// <param name="Roles">割り当てるロールの一覧。<c>null</c> の場合は既定のロールが適用される。</param>
public record CreateUserRequestDto(
    string Email,
    string DisplayName,
    string StoreCode,
    string StoreName,
    IReadOnlyList<string>? Roles);
