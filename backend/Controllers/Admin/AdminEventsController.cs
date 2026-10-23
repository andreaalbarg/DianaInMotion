using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/events")]
[Authorize(Roles = "Admin")]
public class AdminEventsController(AppDb db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var events = await db.Events
            .OrderByDescending(e => e.ScheduledAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                SpeakerPhotoUrl = e.SpeakerPhotoUrl,
                RecordingUrl = e.RecordingUrl,
                IsPast = e.ScheduledAt < DateTime.UtcNow,
                IsFeatured = e.IsFeatured,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);

        return Ok(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventWriteRequest req, CancellationToken ct)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            Title = req.Title,
            Description = req.Description,
            Category = req.Category,
            ScheduledAt = req.ScheduledAt,
            Speaker = req.Speaker,
            SpeakerPhotoUrl = req.SpeakerPhotoUrl,
            RecordingUrl = req.RecordingUrl,
            CreatedAt = DateTime.UtcNow
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync(ct);
        return Ok(new { ev.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EventWriteRequest req, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync([id], ct);
        if (ev is null) return NotFound();

        ev.Title = req.Title;
        ev.Description = req.Description;
        ev.Category = req.Category;
        ev.ScheduledAt = req.ScheduledAt;
        ev.Speaker = req.Speaker;
        ev.SpeakerPhotoUrl = req.SpeakerPhotoUrl;
        ev.RecordingUrl = req.RecordingUrl;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/featured")]
    public async Task<IActionResult> SetFeatured(Guid id, [FromBody] SetFeaturedRequest req, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync([id], ct);
        if (ev is null) return NotFound();

        if (req.IsFeatured)
        {
            var featuredCount = await db.Events
                .CountAsync(e => e.IsFeatured && e.Id != id, ct);
            if (featuredCount >= 3)
                return BadRequest(new ProblemDetails
                {
                    Title = "Ya tienes 3 videos destacados. Quita uno antes de agregar otro."
                });
        }

        ev.IsFeatured = req.IsFeatured;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync([id], ct);
        if (ev is null) return NotFound();
        db.Events.Remove(ev);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record SetFeaturedRequest
{
    public required bool IsFeatured { get; init; }
}
