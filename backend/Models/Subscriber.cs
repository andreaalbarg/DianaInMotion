namespace dianainmotion.Backend.Models;

public class Subscriber
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
