using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDb _db;

    public EventsController(AppDb db) => _db = db;

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var events = await _db.Events
            .Where(e => e.ScheduledAt >= now)
            .OrderBy(e => e.ScheduledAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                RecordingUrl = e.RecordingUrl,
                IsPast = false,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);
        return Ok(events);
    }

    [HttpGet("past")]
    public async Task<IActionResult> GetPast(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var events = await _db.Events
            .Where(e => e.ScheduledAt < now)
            .OrderByDescending(e => e.ScheduledAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                RecordingUrl = e.RecordingUrl,
                IsPast = true,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);
        return Ok(events);
    }

    [HttpPost("{id:guid}/register")]
    public async Task<IActionResult> Register(Guid id, [FromBody] RegisterRequest req, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync(new object[] { id }, ct);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var alreadyRegistered = await _db.Registrations
            .AnyAsync(r => r.EventId == id && r.Email == req.Email, ct);
        if (alreadyRegistered)
            return Conflict(new { message = "Ya estás registrado con este email." });

        _db.Registrations.Add(new Registration
        {
            Id = Guid.NewGuid(),
            EventId = id,
            Name = req.Name,
            Email = req.Email,
            RegisteredAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "¡Registro exitoso! Te enviaremos los detalles por email." });
    }
}
