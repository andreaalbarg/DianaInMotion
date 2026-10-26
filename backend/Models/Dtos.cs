namespace dianainmotion.Backend.Models;

public record EventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public string Language { get; init; } = "es";
    public DateTime ScheduledAt { get; init; }
    public string? Speaker { get; init; }
    public string? SpeakerPhotoUrl { get; init; }
    public string? RecordingUrl { get; init; }
    public string? MeetingUrl { get; init; }
    public bool IsPast { get; init; }
    public bool IsFeatured { get; init; }
    public int RegistrationCount { get; init; }
}

public record RegisterRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

public record SubscribeRequest
{
    public required string Email { get; init; }
}

public record LoginRequest
{
    public required string Password { get; init; }
}

public record LoginResponse
{
    public required string Token { get; init; }
}

public record EventWriteRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public string Language { get; init; } = "es";
    public required DateTime ScheduledAt { get; init; }
    public string? Speaker { get; init; }
    public string? SpeakerPhotoUrl { get; init; }
    public string? RecordingUrl { get; init; }
    public string? MeetingUrl { get; init; }
}

public record RegistrationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public Guid EventId { get; init; }
    public string EventTitle { get; init; } = "";
    public DateTime RegisteredAt { get; init; }
}

public record SubscriberDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = "";
    public DateTime SubscribedAt { get; init; }
}

public record YouTubeSettingsDto
{
    public string ApiKey { get; init; } = "";
    public string ChannelId { get; init; } = "";
}

public record YouTubeSettingsWriteRequest
{
    public required string ApiKey { get; init; }
    public required string ChannelId { get; init; }
}

public record YouTubeVideoPreview
{
    public string VideoId { get; init; } = "";
    public string Title { get; init; } = "";
    public string ThumbnailUrl { get; init; } = "";
    public string Url { get; init; } = "";
}
