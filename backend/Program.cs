using dianainmotion.Backend.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Environment.GetEnvironmentVariable("DIANA_DATA_DIR")
              ?? builder.Environment.ContentRootPath;
Directory.CreateDirectory(dataDir);
var dbPath = Path.Combine(dataDir, "app.db");

builder.Services.AddDbContext<AppDb>(o => o
    .UseSqlite($"Data Source={dbPath}")
    .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning)));

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

// Apply EF migrations on startup
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.Migrate();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("swagger"));
app.MapGet("/api/health", () => new { status = "ok", project = "dianainmotion" });

app.Run();
