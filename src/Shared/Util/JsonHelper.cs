using System.Text.Json;

namespace Shared.Util;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);

    /// <summary>
    /// Deserialize a JSON array string to a list of strings.
    /// Returns an empty list for null or empty input.
    /// </summary>
    public static List<string> DeserializeRoles(string? rolesJson)
    {
        if (string.IsNullOrWhiteSpace(rolesJson))
            return [Constants.Roles.General];

        return RoleHelper.NormalizeRoles(JsonSerializer.Deserialize<List<string>>(rolesJson, Options)).ToList();
    }

    /// <summary>
    /// Serialize a list of role IDs to a JSON array string.
    /// </summary>
    public static string SerializeRoles(IEnumerable<string> roles) =>
        JsonSerializer.Serialize(RoleHelper.NormalizeRoles(roles), Options);
}
