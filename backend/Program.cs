var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:3000", "http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

// Diana runs the app behind nginx, which terminates TLS and proxies over plain http. Trust
// X-Forwarded-Proto so Request.Scheme is https and anything built from it (absolute URLs,
// Swagger's server list, OAuth callbacks) doesn't point at http://.
var __dianaFwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
};
__dianaFwd.KnownIPNetworks.Clear();
__dianaFwd.KnownProxies.Clear();
app.UseForwardedHeaders(__dianaFwd);

// When Diana runs the app behind a reverse-proxy path (e.g. /preview/6100/),
// PATHBASE is injected by PreviewService so the app emits correct URLs.
var pathBase = Environment.GetEnvironmentVariable("PATHBASE");
if (!string.IsNullOrEmpty(pathBase)) app.UsePathBase(pathBase);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("swagger"));
app.MapGet("/api/health", () => new { status = "ok", project = "dianainmotion" });

app.Run();