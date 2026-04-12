using System.Security.Claims;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Repository;
using Shared.Services;
using WebApp.OpenApi;
using WebApp.Options;
using WebApp.Security;
using WebApp.Spa;

var builder = WebApplication.CreateBuilder(args);

// --- Azure Table Storage ---
builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureStorage")
        ?? "UseDevelopmentStorage=true";
    return new TableServiceClient(connectionString);
});

// --- Repositories & Services ---
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton(
    builder.Configuration.GetSection("UserManagement").Get<Shared.Util.UserManagementSettings>()
    ?? new Shared.Util.UserManagementSettings());
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton(sp => new Lazy<IUserService>(() => sp.GetRequiredService<IUserService>()));
builder.Services.Configure<TestLoginOptions>(builder.Configuration.GetSection("TestLogin"));
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = Shared.Util.Constants.AntiforgeryCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.HeaderName = Shared.Util.Constants.XsrfHeaderName;
});
builder.Services.AddScoped<IXsrfTokenCookieService, XsrfTokenCookieService>();

// --- Authentication: Cookie + JWT Bearer (Entra ID) ---
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = Shared.Util.Constants.AuthCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer("EntraID", options =>
    {
        var tenantId = builder.Configuration["AzureAd:TenantId"] ?? "common";
        var clientId = builder.Configuration["AzureAd:ClientId"] ?? "";
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = clientId,
            ValidIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0",
            ValidateIssuer = !string.IsNullOrEmpty(clientId),
            ValidateAudience = !string.IsNullOrEmpty(clientId),
        };
    });

builder.Services.AddAuthorization();

// --- Controllers & OpenAPI ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- SPA static files ---
builder.Services.AddSpaStaticFiles(config =>
{
    config.RootPath = "clientapp/dist";
});

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(OpenApiRoutes.ApiDocumentPath);
}

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseAuthentication();
app.UseMiddleware<XsrfValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.UseWhen(
    context => SpaProxyPathFilter.ShouldProxy(context.Request.Path),
    spaApp => spaApp.UseSpa(spa =>
    {
        spa.Options.SourcePath = "clientapp";

        if (app.Environment.IsDevelopment())
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        }
    }));

app.Run();
