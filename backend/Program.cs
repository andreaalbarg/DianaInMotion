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

// Admin credentials
var adminPassword = builder.Configuration["Admin:Password"]
    ?? throw new InvalidOperationException("Admin:Password not configured");
var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
builder.Services.AddSingleton(new AdminSettings(adminPasswordHash));

// JWT
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret not configured");
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (builder.Environment.IsDevelopment())
    {
        p.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
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

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("swagger"));
app.MapGet("/api/health", () => new { status = "ok", project = "dianainmotion" });

app.Run();

public record AdminSettings(string PasswordHash);
public record JwtSettings(string Secret);
