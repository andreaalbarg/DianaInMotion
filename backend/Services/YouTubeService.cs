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

        var previews = await FetchPreviewsAsync(apiKey, channelId, ct);
        if (previews is null) return null;

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
    /// </summary>
    public async Task<List<YouTubeVideoPreview>?> FetchPreviewsAsync(
        string apiKey, string channelId, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("youtube");

            string uploadsPlaylistId;
            if (!cache.TryGetValue($"yt_playlist_{channelId}", out string? playlistId))
            {
                var channelJson = await client.GetStringAsync(
                    $"channels?part=contentDetails&id={channelId}&key={apiKey}", ct);
                using var channelDoc = JsonDocument.Parse(channelJson, JsonOpts);
                var items = channelDoc.RootElement.GetProperty("items");
                if (items.GetArrayLength() == 0) return null;
                playlistId = items[0]
                    .GetProperty("contentDetails")
                    .GetProperty("relatedPlaylists")
                    .GetProperty("uploads")
                    .GetString()!;
                cache.Set($"yt_playlist_{channelId}", playlistId, TimeSpan.FromDays(7));
            }
            uploadsPlaylistId = playlistId!;

            var playlistJson = await client.GetStringAsync(
                $"playlistItems?part=snippet&maxResults=3&playlistId={uploadsPlaylistId}&key={apiKey}", ct);
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
        catch (Exception ex)
        {
            logger.LogWarning(ex, "YouTube fetch failed.");
            return null;
        }
    }

    public void ClearCache()
    {
        cache.Remove("youtube_latest");
        // Playlist cache is keyed by channel id — cleared on next credential read
    }
}
