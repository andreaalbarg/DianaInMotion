using System.Text;
using dianainmotion.Backend.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// SQLite
var dataDir = Environment.GetEnvironmentVariable("DIANA_DATA_DIR")
              ?? builder.Environment.ContentRootPath;
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "app.db");
builder.Services.AddDbContext<AppDb>(o => o
    .UseSqlite($"Data Source={dbPath}")
    .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning)));

// Admin credentials — auto-generate and persist if not supplied via env/config
var adminPassword = builder.Configuration["Admin:Password"] is { Length: > 0 } ap
    ? ap
    : LoadOrGenerateAdminPassword(dataDir);
var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
builder.Services.AddSingleton(new AdminSettings(adminPasswordHash));

// JWT — auto-generate ephemeral secret if not supplied (tokens lost on restart, acceptable)
var jwtSecret = builder.Configuration["Jwt:Secret"] is { Length: > 0 } js
    ? js
    : Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton(new JwtSettings(jwtSecret));

// MailerDash transactional email (HTTPS REST API)
builder.Services.AddHttpClient("mailerdash");
builder.Services.AddScoped<dianainmotion.Backend.Services.IEmailService, dianainmotion.Backend.Services.EmailService>();
builder.Services.AddHostedService<dianainmotion.Backend.Services.ReminderBackgroundService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("youtube", c =>
    c.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/"));
builder.Services.AddScoped<dianainmotion.Backend.Services.YouTubeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Allow localhost (local dev) and *.app.sourcecoding.ai (Diana preview)
        p.SetIsOriginAllowed(origin =>
         {
             var host = new Uri(origin).Host;
             return host == "localhost" || host.EndsWith(".app.sourcecoding.ai");
         })
         .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    }
    else
    {
        var origins = (builder.Configuration["AllowedOrigins"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (origins.Length > 0)
            p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        else
            p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    }
}));

var app = builder.Build();

// Diana preview: trust forwarded headers from nginx proxy
var __dianaFwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
};
__dianaFwd.KnownIPNetworks.Clear();
__dianaFwd.KnownProxies.Clear();
app.UseForwardedHeaders(__dianaFwd);

var pathBase = Environment.GetEnvironmentVariable("PATHBASE");
if (!string.IsNullOrEmpty(pathBase)) app.UsePathBase(pathBase);

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.Migrate();

// Determine our public URL and write it so the frontend can use it directly.
// Diana passes --urls as a CLI arg; app.Configuration["urls"] captures that.
var boundUrls = app.Configuration["urls"]
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
    ?? "";
if (!string.IsNullOrEmpty(boundUrls) && Uri.TryCreate(boundUrls.Split(';')[0].Trim(), UriKind.Absolute, out var listenUri))
{
    // Proxy discovery file (for Vite proxy)
    File.WriteAllText(Path.Combine(Path.GetTempPath(), "diana_dianainmotion_backend"), $"http://localhost:{listenUri.Port}");

    // Public URL file (for browser-side direct calls, bypasses Vite proxy entirely)
    var pattern = Environment.GetEnvironmentVariable("PreviewSubdomainPattern") ?? "";
    var publicUrl = string.IsNullOrEmpty(pattern) ? "" : pattern.Replace("{port}", listenUri.Port.ToString());
    var workspaceRoot = Path.GetDirectoryName(builder.Environment.ContentRootPath) ?? "";
    var frontendPublic = Path.Combine(workspaceRoot, "frontend", "public");
    if (!string.IsNullOrEmpty(publicUrl) && Directory.Exists(frontendPublic))
        File.WriteAllText(
            Path.Combine(frontendPublic, "backend-config.json"),
            $"{{\"backendUrl\":\"{publicUrl}\"}}");
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("swagger"));
app.MapGet("/api/health", () => new { status = "ok", project = "dianainmotion" });

app.Run();

static string LoadOrGenerateAdminPassword(string dataDir)
{
    var pwFile = Path.Combine(dataDir, "admin_password.txt");
    if (File.Exists(pwFile))
        return File.ReadAllText(pwFile).Trim();

    var pw = Convert.ToBase64String(
        System.Security.Cryptography.RandomNumberGenerator.GetBytes(18))
        .Replace("=", "").Replace("/", "_").Replace("+", "-");
    File.WriteAllText(pwFile, pw);
    Console.WriteLine($"[ADMIN] No Admin:Password configured. Auto-generated password: {pw}  (persisted to {pwFile})");
    return pw;
}

public record AdminSettings(string PasswordHash);
public record JwtSettings(string Secret);
