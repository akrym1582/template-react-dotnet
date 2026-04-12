using System.Text.Json;

namespace Shared.Util;

/// <summary>
/// JSON のシリアライズ・デシリアライズを行うユーティリティクラス。
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// 指定したオブジェクトを JSON 文字列にシリアライズする。
    /// </summary>
    /// <typeparam name="T">シリアライズするオブジェクトの型。</typeparam>
    /// <param name="value">シリアライズするオブジェクト。</param>
    /// <returns>JSON 文字列。</returns>
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options);

    /// <summary>
    /// JSON 文字列を指定した型のオブジェクトにデシリアライズする。
    /// </summary>
    /// <typeparam name="T">デシリアライズ後のオブジェクトの型。</typeparam>
    /// <param name="json">デシリアライズする JSON 文字列。</param>
    /// <returns>デシリアライズされたオブジェクト。失敗した場合は <c>null</c>。</returns>
    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options);

    /// <summary>
    /// ロール ID の JSON 配列文字列をリストにデシリアライズする。
    /// <c>null</c> または空の場合は既定ロール（general）を含むリストを返す。
    /// </summary>
    /// <param name="rolesJson">ロール ID の JSON 配列文字列。</param>
    /// <returns>ロール ID のリスト。</returns>
    public static List<string> DeserializeRoles(string? rolesJson)
    {
        if (string.IsNullOrWhiteSpace(rolesJson))
            return [Constants.Roles.General];

        return RoleHelper.NormalizeRoles(JsonSerializer.Deserialize<List<string>>(rolesJson, Options)).ToList();
    }

    /// <summary>
    /// ロール ID のコレクションを JSON 配列文字列にシリアライズする。
    /// </summary>
    /// <param name="roles">シリアライズするロール ID のコレクション。</param>
    /// <returns>ロール ID の JSON 配列文字列。</returns>
    public static string SerializeRoles(IEnumerable<string> roles) =>
        JsonSerializer.Serialize(RoleHelper.NormalizeRoles(roles), Options);
}
