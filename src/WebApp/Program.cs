using System.Security.Claims;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Shared.Repository;
using Shared.Services;
using WebApp.OpenApi;
using WebApp.Options;
using WebApp.Security;
using WebApp.StaticFiles;

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
        var clientId = builder.Configuration["AzureAd:ClientId"] ?? string.Empty;
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
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(OpenApiRoutes.ApiDocumentPath);
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseMiddleware<XsrfValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    var spaDistPath = Path.Combine(app.Environment.ContentRootPath, "clientapp", "dist");
    var spaStaticFiles = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(spaDistPath),
    };
    var precompressedStaticFileResolver = new PrecompressedStaticFileResolver(spaDistPath);

    app.Use(async (context, next) =>
    {
        if ((HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            && Path.HasExtension(context.Request.Path.Value)
            && precompressedStaticFileResolver.TryResolve(
                context.Request.Path,
                context.Request.Headers.AcceptEncoding,
                out var precompressedFile)
            && precompressedFile is not null)
        {
            context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentEncoding] = precompressedFile.ContentEncoding;
            context.Response.Headers.AppendCommaSeparatedValues(
                Microsoft.Net.Http.Headers.HeaderNames.Vary,
                Microsoft.Net.Http.Headers.HeaderNames.AcceptEncoding);

            await Results.File(
                precompressedFile.PhysicalPath,
                precompressedFile.ContentType,
                lastModified: precompressedFile.LastModified,
                enableRangeProcessing: true).ExecuteAsync(context);

            return;
        }

        await next();
    });

    app.UseStaticFiles(spaStaticFiles);
    app.MapFallback(async context =>
    {
        const string SpaIndexPath = "/index.html";

        if (precompressedStaticFileResolver.TryResolve(
            SpaIndexPath,
            context.Request.Headers.AcceptEncoding,
            out var precompressedFile)
            && precompressedFile is not null)
        {
            context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentEncoding] = precompressedFile.ContentEncoding;
            context.Response.Headers.AppendCommaSeparatedValues(
                Microsoft.Net.Http.Headers.HeaderNames.Vary,
                Microsoft.Net.Http.Headers.HeaderNames.AcceptEncoding);

            await Results.File(
                precompressedFile.PhysicalPath,
                precompressedFile.ContentType,
                lastModified: precompressedFile.LastModified,
                enableRangeProcessing: true).ExecuteAsync(context);

            return;
        }

        await Results.File(
            Path.Combine(spaDistPath, "index.html"),
            "text/html; charset=utf-8",
            lastModified: File.GetLastWriteTimeUtc(Path.Combine(spaDistPath, "index.html"))).ExecuteAsync(context);
    });
}

app.Run();
