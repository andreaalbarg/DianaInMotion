namespace dianainmotion.Backend.Models;

public class Registration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReminderSentAt { get; set; }

    public Event Event { get; set; } = null!;
}
