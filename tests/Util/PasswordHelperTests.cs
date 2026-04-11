using Shared.Util;

namespace Tests.Util;

public class PasswordHelperTests
{
    [Fact]
    public void Hash_And_Verify_CorrectPassword_ReturnsTrue()
    {
        var password = "TestPassword123!";
        var hash = PasswordHelper.Hash(password);
        Assert.True(PasswordHelper.Verify(password, hash));
    }

    [Fact]
    public void Hash_And_Verify_WrongPassword_ReturnsFalse()
    {
        var password = "TestPassword123!";
        var hash = PasswordHelper.Hash(password);
        Assert.False(PasswordHelper.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Hash_GeneratesDifferentHashes_ForSamePassword()
    {
        var password = "TestPassword123!";
        var hash1 = PasswordHelper.Hash(password);
        var hash2 = PasswordHelper.Hash(password);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_InvalidHashFormat_ReturnsFalse()
    {
        Assert.False(PasswordHelper.Verify("password", "invalidhash"));
    }
}
