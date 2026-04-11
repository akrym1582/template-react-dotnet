namespace Shared.Util;

public class UserManagementSettings
{
    public bool AllowManagerUserCreation { get; set; }
    public string InitialPassword { get; set; } = "Init@1234";
    public PasswordPolicySettings PasswordPolicy { get; set; } = new();
}

public class PasswordPolicySettings
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialCharacter { get; set; } = true;
}
