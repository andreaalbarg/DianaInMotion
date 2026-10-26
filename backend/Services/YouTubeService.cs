using System.Text.Json;
using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace dianainmotion.Backend.Services;

public class YouTubeService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration config,
    ILogger<YouTubeService> logger,
    AppDb db)
{
    private static readonly JsonDocumentOptions JsonOpts = new() { AllowTrailingCommas = true };

    private async Task<(string apiKey, string channelId)> GetCredentialsAsync(CancellationToken ct)
    {
        var apiKey = await db.AppSettings
            .Where(s => s.Key == "YouTube:ApiKey")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct)
            ?? config["YouTube:ApiKey"]
            ?? "";

        var channelId = await db.AppSettings
            .Where(s => s.Key == "YouTube:ChannelId")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct)
            ?? config["YouTube:ChannelId"]
            ?? "";

        return (apiKey, channelId);
    }

    /// <summary>
    /// Returns the 3 most recent public videos from the configured YouTube channel,
    /// cached for 1 hour. Returns null if YouTube is not configured.
    /// </summary>
    public async Task<List<EventDto>?> GetLatestVideosAsync(CancellationToken ct)
    {
        var (apiKey, channelId) = await GetCredentialsAsync(ct);

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(channelId))
            return null;

        if (cache.TryGetValue("youtube_latest", out List<EventDto>? cached))
            return cached;

        List<YouTubeVideoPreview> previews;
        try { previews = await FetchPreviewsAsync(apiKey, channelId, ct); }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "YouTube fetch failed, falling back to DB featured events.");
            return null;
        }

        var videos = previews.Select(p => new EventDto
        {
            Id = Guid.NewGuid(),
            Title = p.Title,
            Description = "",
            Category = "basics",
            ScheduledAt = DateTime.UtcNow.AddDays(-1),
            RecordingUrl = p.Url,
            IsPast = true,
            IsFeatured = true,
            RegistrationCount = 0,
        }).ToList();

        cache.Set("youtube_latest", videos, TimeSpan.FromHours(1));
        return videos;
    }

    /// <summary>
    /// Fetches video previews directly (bypasses cache) for admin testing.
    /// Throws <see cref="InvalidOperationException"/> with a user-facing message on failure.
    /// </summary>
    public async Task<List<YouTubeVideoPreview>> FetchPreviewsAsync(
        string apiKey, string channelId, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("youtube");

        string uploadsPlaylistId;
        if (!cache.TryGetValue($"yt_playlist_{channelId}", out string? playlistId))
        {
            HttpResponseMessage channelResp;
            try
            {
                channelResp = await client.GetAsync(
                    $"channels?part=contentDetails&id={channelId}&key={apiKey}", ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "YouTube channels request failed.");
                throw new InvalidOperationException("No se pudo conectar con YouTube. Verifica tu conexión a Internet.");
            }

            var channelJson = await channelResp.Content.ReadAsStringAsync(ct);
            if (!channelResp.IsSuccessStatusCode)
            {
                // Extract YouTube's error message if available
                var ytError = "";
                try
                {
                    using var errDoc = JsonDocument.Parse(channelJson, JsonOpts);
                    ytError = errDoc.RootElement
                        .GetProperty("error").GetProperty("message").GetString() ?? "";
                }
                catch { }
                logger.LogWarning("YouTube API error {Status}: {Body}", channelResp.StatusCode, channelJson);
                throw new InvalidOperationException(
                    string.IsNullOrEmpty(ytError)
                        ? $"YouTube rechazó la solicitud (HTTP {(int)channelResp.StatusCode}). Verifica el API Key."
                        : $"YouTube: {ytError}");
            }

            using var channelDoc = JsonDocument.Parse(channelJson, JsonOpts);
            var items = channelDoc.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0)
                throw new InvalidOperationException(
                    "No se encontró el canal. Asegúrate de usar el ID del canal (empieza con 'UC'), no el nombre o @handle.");

            playlistId = items[0]
                .GetProperty("contentDetails")
                .GetProperty("relatedPlaylists")
                .GetProperty("uploads")
                .GetString()!;
            cache.Set($"yt_playlist_{channelId}", playlistId, TimeSpan.FromDays(7));
        }
        uploadsPlaylistId = playlistId!;

        HttpResponseMessage playlistResp;
        try
        {
            playlistResp = await client.GetAsync(
                $"playlistItems?part=snippet&maxResults=3&playlistId={uploadsPlaylistId}&key={apiKey}", ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "YouTube playlistItems request failed.");
            throw new InvalidOperationException("No se pudo obtener los videos del canal. Verifica tu conexión a Internet.");
        }

        var playlistJson = await playlistResp.Content.ReadAsStringAsync(ct);
        if (!playlistResp.IsSuccessStatusCode)
        {
            var ytError = "";
            try
            {
                using var errDoc = JsonDocument.Parse(playlistJson, JsonOpts);
                ytError = errDoc.RootElement
                    .GetProperty("error").GetProperty("message").GetString() ?? "";
            }
            catch { }
            logger.LogWarning("YouTube playlistItems error {Status}: {Body}", playlistResp.StatusCode, playlistJson);
            throw new InvalidOperationException(
                string.IsNullOrEmpty(ytError)
                    ? $"Error al obtener los videos (HTTP {(int)playlistResp.StatusCode}). Verifica el API Key."
                    : $"YouTube: {ytError}");
        }

        using var playlistDoc = JsonDocument.Parse(playlistJson, JsonOpts);

        return playlistDoc.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(item =>
            {
                var snippet = item.GetProperty("snippet");
                var videoId = snippet.GetProperty("resourceId").GetProperty("videoId").GetString()!;
                var title = snippet.GetProperty("title").GetString() ?? "";
                var thumbnail = snippet.TryGetProperty("thumbnails", out var thumbs)
                    && thumbs.TryGetProperty("medium", out var medium)
                    && medium.TryGetProperty("url", out var url)
                    ? url.GetString() ?? ""
                    : "";

                return new YouTubeVideoPreview
                {
                    VideoId = videoId,
                    Title = title,
                    ThumbnailUrl = thumbnail,
                    Url = $"https://www.youtube.com/watch?v={videoId}",
                };
            })
            .ToList();
    }

    public void ClearCache()
    {
        cache.Remove("youtube_latest");
        // Playlist cache is keyed by channel id — cleared on next credential read
    }
}
