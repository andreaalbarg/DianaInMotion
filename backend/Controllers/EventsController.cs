using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using dianainmotion.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers;

[ApiController]
[Route("api/events")]
[AllowAnonymous]
public class EventsController(AppDb db, YouTubeService youtube, IEmailService emailService) : ControllerBase
{

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var events = await db.Events
            .Where(e => e.ScheduledAt >= now)
            .OrderBy(e => e.ScheduledAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                Language = e.Language,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                SpeakerPhotoUrl = e.SpeakerPhotoUrl,
                RecordingUrl = e.RecordingUrl,
                MeetingUrl = e.MeetingUrl,
                IsPast = false,
                IsFeatured = e.IsFeatured,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);
        return Ok(events);
    }

    [HttpGet("past")]
    public async Task<IActionResult> GetPast(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var events = await db.Events
            .Where(e => e.ScheduledAt < now)
            .OrderByDescending(e => e.ScheduledAt)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                Language = e.Language,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                SpeakerPhotoUrl = e.SpeakerPhotoUrl,
                RecordingUrl = e.RecordingUrl,
                MeetingUrl = e.MeetingUrl,
                IsPast = true,
                IsFeatured = e.IsFeatured,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);
        return Ok(events);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        // Try YouTube channel first; fall back to manually-starred DB events
        var ytVideos = await youtube.GetLatestVideosAsync(ct);
        if (ytVideos is { Count: > 0 })
            return Ok(ytVideos);

        var now = DateTime.UtcNow;
        var events = await db.Events
            .Where(e => e.ScheduledAt < now && e.IsFeatured)
            .OrderByDescending(e => e.ScheduledAt)
            .Take(3)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Category = e.Category,
                Language = e.Language,
                ScheduledAt = e.ScheduledAt,
                Speaker = e.Speaker,
                SpeakerPhotoUrl = e.SpeakerPhotoUrl,
                RecordingUrl = e.RecordingUrl,
                MeetingUrl = e.MeetingUrl,
                IsPast = true,
                IsFeatured = true,
                RegistrationCount = e.Registrations.Count()
            })
            .ToListAsync(ct);
        return Ok(events);
    }

    [HttpPost("{id:guid}/register")]
    public async Task<IActionResult> Register(Guid id, [FromBody] RegisterRequest req, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync(new object[] { id }, ct);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var alreadyRegistered = await db.Registrations
            .AnyAsync(r => r.EventId == id && r.Email == req.Email, ct);
        if (alreadyRegistered)
            return Conflict(new { message = "Ya estás registrado con este email." });

        db.Registrations.Add(new Registration
        {
            Id = Guid.NewGuid(),
            EventId = id,
            Name = req.Name,
            Email = req.Email,
            RegisteredAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        _ = emailService.SendConfirmationAsync(req.Email, req.Name, ev.Title, ev.ScheduledAt, ev.MeetingUrl, ev.Language, ct);

        return Ok(new { message = "¡Registro exitoso! Te enviaremos los detalles por email." });
    }
}
