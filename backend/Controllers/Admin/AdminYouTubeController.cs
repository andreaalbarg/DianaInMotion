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
public class AdminYouTubeController(AppDb db, YouTubeService youtube) : ControllerBase
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

    [HttpGet("preview")]
    public async Task<IActionResult> Preview(CancellationToken ct)
    {
        var apiKey = await db.AppSettings
            .Where(s => s.Key == "YouTube:ApiKey")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct) ?? "";

        var channelId = await db.AppSettings
            .Where(s => s.Key == "YouTube:ChannelId")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct) ?? "";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(channelId))
            return BadRequest(new ProblemDetails { Title = "YouTube no está configurado." });

        var videos = await youtube.FetchPreviewsAsync(apiKey, channelId, ct);
        if (videos is null)
            return BadRequest(new ProblemDetails { Title = "No se pudo conectar con YouTube. Verifica el API Key y el Canal." });

        return Ok(videos);
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
