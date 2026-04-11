using Shared.Util;

namespace Tests.Util;

public class JsonHelperTests
{
    [Fact]
    public void DeserializeRoles_WithValidJson_ReturnsRoles()
    {
        var json = "[\"admin\",\"user\"]";
        var roles = JsonHelper.DeserializeRoles(json);
        Assert.Equal(2, roles.Count);
        Assert.Contains("admin", roles);
        Assert.Contains("user", roles);
    }

    [Fact]
    public void DeserializeRoles_WithNull_ReturnsEmptyList()
    {
        var roles = JsonHelper.DeserializeRoles(null);
        Assert.Empty(roles);
    }

    [Fact]
    public void DeserializeRoles_WithEmptyString_ReturnsEmptyList()
    {
        var roles = JsonHelper.DeserializeRoles("");
        Assert.Empty(roles);
    }

    [Fact]
    public void SerializeRoles_ReturnsJsonArray()
    {
        var roles = new[] { "admin", "editor" };
        var json = JsonHelper.SerializeRoles(roles);
        Assert.Equal("[\"admin\",\"editor\"]", json);
    }

    [Fact]
    public void Serialize_Deserialize_Roundtrip()
    {
        var data = new { Name = "test", Value = 42 };
        var json = JsonHelper.Serialize(data);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"value\"", json);
    }
}
