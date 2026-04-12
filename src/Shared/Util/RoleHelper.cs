using System.Security.Claims;

namespace Shared.Util;

/// <summary>
/// ロールの正規化と権限チェックを行うユーティリティクラス。
/// </summary>
public static class RoleHelper
{
    /// <summary>
    /// ロール ID のコレクションを正規化して重複を除去する。
    /// 有効なロールが存在しない場合は既定ロール（general）を返す。
    /// </summary>
    /// <param name="roles">正規化するロール ID のコレクション。</param>
    /// <returns>正規化されたロール ID の読み取り専用リスト。</returns>
    public static IReadOnlyList<string> NormalizeRoles(IEnumerable<string>? roles)
    {
        var normalizedRoles = roles?
            .Select(NormalizeRole)
            .Where(role => role is not null)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalizedRoles is { Count: > 0 } ? normalizedRoles : [Constants.Roles.General];
    }

    /// <summary>
    /// ロール ID のコレクションからユーザー管理権限（manager 以上）を持つか判定する。
    /// </summary>
    /// <param name="roles">判定するロール ID のコレクション。</param>
    /// <returns>ユーザー管理権限を持つ場合は <c>true</c>、それ以外は <c>false</c>。</returns>
    public static bool CanManageUsers(IEnumerable<string>? roles) =>
        NormalizeRoles(roles).Any(IsManagerOrAboveRole);

    /// <summary>
    /// <see cref="ClaimsPrincipal"/> からユーザー管理権限（manager 以上）を持つか判定する。
    /// </summary>
    /// <param name="user">判定する <see cref="ClaimsPrincipal"/>。</param>
    /// <returns>ユーザー管理権限を持つ場合は <c>true</c>、それ以外は <c>false</c>。</returns>
    public static bool CanManageUsers(ClaimsPrincipal user) =>
        user.IsInRole(Constants.Roles.Manager) || user.IsInRole(Constants.Roles.Privileged);

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        var normalizedRole = role.Trim();

        if (string.Equals(normalizedRole, "user", StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Roles.General;
        }

        if (string.Equals(normalizedRole, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Roles.Privileged;
        }

        if (string.Equals(normalizedRole, Constants.Roles.General, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Roles.General;
        }

        if (string.Equals(normalizedRole, Constants.Roles.Manager, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Roles.Manager;
        }

        if (string.Equals(normalizedRole, Constants.Roles.Privileged, StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Roles.Privileged;
        }

        return null;
    }

    private static bool IsManagerOrAboveRole(string? role) =>
        string.Equals(role, Constants.Roles.Manager, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, Constants.Roles.Privileged, StringComparison.OrdinalIgnoreCase);
}
