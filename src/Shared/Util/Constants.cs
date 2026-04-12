namespace Shared.Util;

public static class Constants
{
    public const string UserPartitionKey = "USER";
    public const string UsersTableName = "Users";
    public const string AuthCookieName = ".TemplateApp.Auth";
    public const string AntiforgeryCookieName = ".TemplateApp.Antiforgery";
    public const string XsrfTokenCookieName = "XSRF-TOKEN";
    public const string XsrfHeaderName = "X-XSRF-TOKEN";

    public static class Roles
    {
        public const string General = "general";
        public const string Manager = "manager";
        public const string Privileged = "privileged";
        public const string ManageableRolesCsv = $"{Manager},{Privileged}";
    }
}
