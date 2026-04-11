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

    public static bool IsManagerOrAbove(string role) =>
        IsManagerOrAboveRole(NormalizeRole(role));

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return null;

        return role.Trim().ToLowerInvariant() switch
        {
            "user" => Constants.Roles.General,
            "admin" => Constants.Roles.Privileged,
            Constants.Roles.General => Constants.Roles.General,
            Constants.Roles.Manager => Constants.Roles.Manager,
            Constants.Roles.Privileged => Constants.Roles.Privileged,
            _ => null
        };
    }

    private static bool IsManagerOrAboveRole(string? role) =>
        string.Equals(role, Constants.Roles.Manager, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, Constants.Roles.Privileged, StringComparison.OrdinalIgnoreCase);
}
