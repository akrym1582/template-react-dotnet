using Azure;
using Azure.Data.Tables;

namespace Shared.Models;

/// <summary>
/// Azure Table Storage entity for user information.
/// PartitionKey = "USER", RowKey = UserId (GUID string).
/// Roles are stored as a JSON array of role ID strings.
/// </summary>
public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "USER";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of role IDs, e.g. ["admin","editor"]
    /// </summary>
    public string RolesJson { get; set; } = "[]";

    /// <summary>
    /// Azure Entra ID object ID (null for local accounts).
    /// </summary>
    public string? EntraObjectId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
