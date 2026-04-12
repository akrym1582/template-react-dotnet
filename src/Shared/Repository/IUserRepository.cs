using Shared.Models;

namespace Shared.Repository;

/// <summary>
/// ユーザーエンティティへのデータアクセスを提供するリポジトリインターフェース。
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// 指定した ID のユーザーを取得する。
    /// </summary>
    /// <param name="userId">ユーザー ID（GUID 文字列）。</param>
    /// <returns>ユーザーエンティティ。見つからない場合は <c>null</c>。</returns>
    Task<UserEntity?> GetByIdAsync(string userId);

    /// <summary>
    /// 指定したメールアドレスのユーザーを取得する。
    /// </summary>
    /// <param name="email">メールアドレス。</param>
    /// <returns>ユーザーエンティティ。見つからない場合は <c>null</c>。</returns>
    Task<UserEntity?> GetByEmailAsync(string email);

    /// <summary>
    /// 指定した Azure Entra ID オブジェクト ID のユーザーを取得する。
    /// </summary>
    /// <param name="entraObjectId">Azure Entra ID のオブジェクト ID。</param>
    /// <returns>ユーザーエンティティ。見つからない場合は <c>null</c>。</returns>
    Task<UserEntity?> GetByEntraObjectIdAsync(string entraObjectId);

    /// <summary>
    /// 全ユーザーを取得する。
    /// </summary>
    /// <returns>ユーザーエンティティの一覧。</returns>
    Task<IReadOnlyList<UserEntity>> GetAllAsync();

    /// <summary>
    /// ユーザーを作成または更新する（Upsert）。
    /// </summary>
    /// <param name="user">保存するユーザーエンティティ。</param>
    Task UpsertAsync(UserEntity user);

    /// <summary>
    /// 指定した ID のユーザーを削除する。
    /// </summary>
    /// <param name="userId">削除するユーザーの ID（GUID 文字列）。</param>
    Task DeleteAsync(string userId);
}
