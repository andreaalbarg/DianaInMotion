namespace dianainmotion.Backend.Models;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "basics"; // "basics" | "advanced"
    public DateTime ScheduledAt { get; set; }
    public string? Speaker { get; set; }
    public string? SpeakerPhotoUrl { get; set; }
    public string? RecordingUrl { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
