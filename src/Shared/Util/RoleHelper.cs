using System.Security.Claims;

namespace Shared.Util;

public static class RoleHelper
{
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

    public static bool CanManageUsers(IEnumerable<string>? roles) =>
        NormalizeRoles(roles).Any(IsManagerOrAboveRole);

    public static bool CanManageUsers(ClaimsPrincipal user) =>
        user.IsInRole(Constants.Roles.Manager) || user.IsInRole(Constants.Roles.Privileged);

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return null;

        var normalizedRole = role.Trim();

        if (string.Equals(normalizedRole, "user", StringComparison.OrdinalIgnoreCase))
            return Constants.Roles.General;

        if (string.Equals(normalizedRole, "admin", StringComparison.OrdinalIgnoreCase))
            return Constants.Roles.Privileged;

        if (string.Equals(normalizedRole, Constants.Roles.General, StringComparison.OrdinalIgnoreCase))
            return Constants.Roles.General;

        if (string.Equals(normalizedRole, Constants.Roles.Manager, StringComparison.OrdinalIgnoreCase))
            return Constants.Roles.Manager;

        if (string.Equals(normalizedRole, Constants.Roles.Privileged, StringComparison.OrdinalIgnoreCase))
            return Constants.Roles.Privileged;

        return null;
    }

    private static bool IsManagerOrAboveRole(string? role) =>
        string.Equals(role, Constants.Roles.Manager, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, Constants.Roles.Privileged, StringComparison.OrdinalIgnoreCase);
}
