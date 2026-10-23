using System.Text.Json;
using dianainmotion.Backend.Models;
using Microsoft.Extensions.Caching.Memory;

namespace dianainmotion.Backend.Services;

public class YouTubeService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration config,
    ILogger<YouTubeService> logger)
{
    private static readonly JsonDocumentOptions JsonOpts = new() { AllowTrailingCommas = true };

    /// <summary>
    /// Returns the 3 most recent public videos from the configured YouTube channel,
    /// cached for 1 hour. Returns null if YouTube is not configured.
    /// </summary>
    public async Task<List<EventDto>?> GetLatestVideosAsync(CancellationToken ct)
    {
        var apiKey = config["YouTube:ApiKey"];
        var channelId = config["YouTube:ChannelId"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(channelId))
            return null;

        if (cache.TryGetValue("youtube_latest", out List<EventDto>? cached))
            return cached;

        try
        {
            var client = httpClientFactory.CreateClient("youtube");

            // 1) resolve the uploads playlist (costs 1 quota unit, cached separately)
            string uploadsPlaylistId;
            if (!cache.TryGetValue("youtube_uploads_playlist", out string? playlistId))
            {
                var channelJson = await client.GetStringAsync(
                    $"channels?part=contentDetails&id={channelId}&key={apiKey}", ct);
                using var channelDoc = JsonDocument.Parse(channelJson, JsonOpts);
                playlistId = channelDoc.RootElement
                    .GetProperty("items")[0]
                    .GetProperty("contentDetails")
                    .GetProperty("relatedPlaylists")
                    .GetProperty("uploads")
                    .GetString()!;
                cache.Set("youtube_uploads_playlist", playlistId, TimeSpan.FromDays(7));
            }
            uploadsPlaylistId = playlistId!;

            // 2) fetch latest 3 videos from the uploads playlist (costs 1 quota unit)
            var playlistJson = await client.GetStringAsync(
                $"playlistItems?part=snippet&maxResults=3&playlistId={uploadsPlaylistId}&key={apiKey}", ct);
            using var playlistDoc = JsonDocument.Parse(playlistJson, JsonOpts);

            var videos = playlistDoc.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Select(item =>
                {
                    var snippet = item.GetProperty("snippet");
                    var videoId = snippet.GetProperty("resourceId").GetProperty("videoId").GetString()!;
                    var title = snippet.GetProperty("title").GetString() ?? "";
                    var desc = snippet.GetProperty("description").GetString() ?? "";

                    return new EventDto
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Description = desc.Length > 300 ? desc[..300] + "…" : desc,
                        Category = "basics",
                        ScheduledAt = DateTime.UtcNow.AddDays(-1),
                        RecordingUrl = $"https://www.youtube.com/watch?v={videoId}",
                        IsPast = true,
                        IsFeatured = true,
                        RegistrationCount = 0,
                    };
                })
                .ToList();

            cache.Set("youtube_latest", videos, TimeSpan.FromHours(1));
            return videos;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "YouTube fetch failed, falling back to DB featured events.");
            return null;
        }
    }
}
