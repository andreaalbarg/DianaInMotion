namespace dianainmotion.Backend.Models;

public record EventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public DateTime ScheduledAt { get; init; }
    public string? Speaker { get; init; }
    public string? SpeakerPhotoUrl { get; init; }
    public string? RecordingUrl { get; init; }
    public bool IsPast { get; init; }
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
