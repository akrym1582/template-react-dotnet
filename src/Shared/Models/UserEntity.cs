using Azure;
using Azure.Data.Tables;

namespace Shared.Models;

/// <summary>
/// Azure Table Storage に保存するユーザー情報のエンティティ。
/// PartitionKey = "USER"、RowKey = UserId（GUID 文字列）。
/// ロールは JSON 配列文字列として保存される。
/// </summary>
public class UserEntity : ITableEntity
{
    /// <summary>パーティションキー。常に "USER" を使用する。</summary>
    public string PartitionKey { get; set; } = "USER";

    /// <summary>行キー。ユーザーの一意識別子（GUID 文字列）。</summary>
    public string RowKey { get; set; } = string.Empty;

    /// <summary>エンティティのタイムスタンプ（Azure Table Storage が自動設定）。</summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>楽観的同時実行制御のための ETag。</summary>
    public ETag ETag { get; set; }

    /// <summary>メールアドレス。</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>表示名。</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>ハッシュ化されたパスワード（PBKDF2 形式）。</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>店舗コード。</summary>
    public string StoreCode { get; set; } = string.Empty;

    /// <summary>店舗名。</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// ロール ID の JSON 配列文字列。例: ["general","manager"]
    /// </summary>
    public string RolesJson { get; set; } = "[]";

    /// <summary>
    /// Azure Entra ID のオブジェクト ID。ローカルアカウントの場合は <c>null</c>。
    /// </summary>
    public string? EntraObjectId { get; set; }

    /// <summary>アカウントが有効かどうか。</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>次回ログイン時にパスワード変更が必要かどうか。</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>アカウント作成日時（UTC）。</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>最終ログイン日時（UTC）。未ログインの場合は <c>null</c>。</summary>
    public DateTime? LastLoginAt { get; set; }
}
