namespace Shared.Util;

public static class Constants
{
    public const string UserPartitionKey = "USER";
    public const string UsersTableName = "Users";
    public const string AuthCookieName = ".TemplateApp.Auth";

    public static class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }
}
