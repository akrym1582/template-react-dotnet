using Shared.Dto;

namespace Shared.Services;

/// <summary>
/// ユーザー管理に関するビジネスロジックを提供するサービスインターフェース。
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 指定した ID のユーザーを取得する。
    /// </summary>
    /// <param name="userId">ユーザー ID（GUID 文字列）。</param>
    /// <returns>ユーザー DTO。見つからない場合は <c>null</c>。</returns>
    Task<UserDto?> GetByIdAsync(string userId);

    /// <summary>
    /// 指定したメールアドレスのユーザーを取得する。
    /// </summary>
    /// <param name="email">メールアドレス。</param>
    /// <returns>ユーザー DTO。見つからない場合は <c>null</c>。</returns>
    Task<UserDto?> GetByEmailAsync(string email);

    /// <summary>
    /// 全ユーザーを取得する。
    /// </summary>
    /// <returns>ユーザー DTO の一覧。</returns>
    Task<IReadOnlyList<UserDto>> GetAllAsync();

    /// <summary>
    /// 新しいユーザーを作成する。
    /// </summary>
    /// <param name="email">メールアドレス。</param>
    /// <param name="displayName">表示名。</param>
    /// <param name="password">初期パスワード（平文）。</param>
    /// <param name="storeCode">店舗コード。</param>
    /// <param name="storeName">店舗名。</param>
    /// <param name="roles">割り当てるロールの一覧。<c>null</c> の場合は既定のロールが適用される。</param>
    /// <param name="mustChangePassword">次回ログイン時にパスワード変更を強制するかどうか。</param>
    /// <returns>作成されたユーザーの DTO。</returns>
    Task<UserDto> CreateAsync(
        string email,
        string displayName,
        string password,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null,
        bool mustChangePassword = false);

    /// <summary>
    /// ユーザー情報を更新する。
    /// </summary>
    /// <param name="userId">更新対象のユーザー ID。</param>
    /// <param name="email">新しいメールアドレス。</param>
    /// <param name="displayName">新しい表示名。</param>
    /// <param name="storeCode">新しい店舗コード。</param>
    /// <param name="storeName">新しい店舗名。</param>
    /// <param name="roles">新しいロールの一覧。<c>null</c> の場合はロールを変更しない。</param>
    /// <returns>更新後のユーザー DTO。ユーザーが見つからない場合は <c>null</c>。</returns>
    Task<UserDto?> UpdateAsync(
        string userId,
        string email,
        string displayName,
        string storeCode,
        string storeName,
        IEnumerable<string>? roles = null);

    /// <summary>
    /// メールアドレスとパスワードで認証を行う。
    /// </summary>
    /// <param name="email">メールアドレス。</param>
    /// <param name="password">パスワード（平文）。</param>
    /// <returns>認証成功時はユーザー DTO、失敗時は <c>null</c>。</returns>
    Task<UserDto?> ValidateCredentialsAsync(string email, string password);

    /// <summary>
    /// Azure Entra ID ユーザーを取得、または存在しない場合は新規作成する。
    /// </summary>
    /// <param name="entraObjectId">Azure Entra ID のオブジェクト ID。</param>
    /// <param name="email">メールアドレス。</param>
    /// <param name="displayName">表示名。</param>
    /// <returns>ユーザー DTO。</returns>
    Task<UserDto> GetOrCreateEntraUserAsync(string entraObjectId, string email, string displayName);

    /// <summary>
    /// ユーザーのパスワードを変更する。
    /// </summary>
    /// <param name="userId">対象のユーザー ID。</param>
    /// <param name="newPassword">新しいパスワード（平文）。</param>
    /// <returns>更新後のユーザー DTO。ユーザーが見つからない場合は <c>null</c>。</returns>
    Task<UserDto?> ChangePasswordAsync(string userId, string newPassword);

    /// <summary>
    /// ユーザーのパスワードを初期パスワードにリセットする。
    /// </summary>
    /// <param name="userId">対象のユーザー ID。</param>
    /// <returns>リセット結果 DTO。ユーザーが見つからない場合は <c>null</c>。</returns>
    Task<PasswordResetResultDto?> ResetPasswordAsync(string userId);

    /// <summary>
    /// パスワードポリシーに基づいてパスワードを検証する。
    /// </summary>
    /// <param name="password">検証するパスワード（平文）。</param>
    /// <returns>ポリシー違反がある場合はエラーメッセージ、問題なければ <c>null</c>。</returns>
    Task<string?> ValidatePasswordPolicyAsync(string password);

    /// <summary>
    /// 指定した ID のユーザーを削除する。
    /// </summary>
    /// <param name="userId">削除するユーザーの ID。</param>
    Task DeleteAsync(string userId);
}
