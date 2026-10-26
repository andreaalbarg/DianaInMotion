using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using dianainmotion.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/youtube")]
[Authorize]
public class AdminYouTubeController(AppDb db, YouTubeService youtube, ILogger<AdminYouTubeController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var apiKey = await db.AppSettings
            .Where(s => s.Key == "YouTube:ApiKey")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct) ?? "";

        var channelId = await db.AppSettings
            .Where(s => s.Key == "YouTube:ChannelId")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct) ?? "";

        return Ok(new YouTubeSettingsDto { ApiKey = apiKey, ChannelId = channelId });
    }

    [HttpPut]
    public async Task<IActionResult> SaveSettings([FromBody] YouTubeSettingsWriteRequest req, CancellationToken ct)
    {
        await Upsert("YouTube:ApiKey", req.ApiKey, ct);
        await Upsert("YouTube:ChannelId", req.ChannelId, ct);
        await db.SaveChangesAsync(ct);
        youtube.ClearCache();
        return NoContent();
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] YouTubeSettingsWriteRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ApiKey) || string.IsNullOrWhiteSpace(req.ChannelId))
            return BadRequest(new ProblemDetails { Title = "Ingresa el API Key y el ID del canal antes de probar." });

        if (req.ChannelId.StartsWith("http", StringComparison.OrdinalIgnoreCase) || req.ChannelId.Contains('/'))
            return BadRequest(new ProblemDetails { Title = "Ingresa el ID del canal (empieza con 'UC') o el @handle (ejemplo: @DianaInMotion), no la URL completa." });

        try
        {
            var videos = await youtube.FetchPreviewsAsync(req.ApiKey, req.ChannelId, ct);
            return Ok(videos);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Unexpected error testing YouTube connection.");
            return BadRequest(new ProblemDetails { Title = "Error inesperado al conectar con YouTube. Verifica el API Key y el ID del canal." });
        }
    }

    private async Task Upsert(string key, string value, CancellationToken ct)
    {
        var existing = await db.AppSettings.FindAsync([key], ct);
        if (existing is null)
            db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        else
            existing.Value = value;
    }
}
