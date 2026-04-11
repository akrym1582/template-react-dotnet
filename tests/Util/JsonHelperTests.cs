using Shared.Util;

namespace Tests.Util;

public class JsonHelperTests
{
    [Fact]
    public void DeserializeRoles_WithValidJson_ReturnsRoles()
    {
        var json = "[\"privileged\",\"general\"]";
        var roles = JsonHelper.DeserializeRoles(json);
        Assert.Equal(2, roles.Count);
        Assert.Contains("privileged", roles);
        Assert.Contains("general", roles);
    }

    [Fact]
    public void DeserializeRoles_WithLegacyJson_NormalizesRoles()
    {
        var json = "[\"admin\",\"user\"]";
        var roles = JsonHelper.DeserializeRoles(json);
        Assert.Equal(["privileged", "general"], roles);
    }

    [Fact]
    public void DeserializeRoles_WithNull_ReturnsGeneralRole()
    {
        var roles = JsonHelper.DeserializeRoles(null);
        Assert.Equal(["general"], roles);
    }

    [Fact]
    public void DeserializeRoles_WithEmptyString_ReturnsGeneralRole()
    {
        var roles = JsonHelper.DeserializeRoles("");
        Assert.Equal(["general"], roles);
    }

    [Fact]
    public void SerializeRoles_ReturnsJsonArray()
    {
        var roles = new[] { "privileged", "manager" };
        var json = JsonHelper.SerializeRoles(roles);
        Assert.Equal("[\"privileged\",\"manager\"]", json);
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
