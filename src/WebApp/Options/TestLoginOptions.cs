namespace WebApp.Options;

public class TestLoginOptions
{
    public List<TestLoginUserOption> Users { get; set; } = [];
}

public class TestLoginUserOption
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
