using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/registrations")]
[Authorize(Roles = "Admin")]
public class AdminRegistrationsController(AppDb db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? eventId, CancellationToken ct)
    {
        var query = db.Registrations.Include(r => r.Event).AsQueryable();
        if (eventId.HasValue)
            query = query.Where(r => r.EventId == eventId.Value);

        var registrations = await query
            .OrderByDescending(r => r.RegisteredAt)
            .Select(r => new RegistrationDto
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                EventId = r.EventId,
                EventTitle = r.Event.Title,
                RegisteredAt = r.RegisteredAt
            })
            .ToListAsync(ct);

        return Ok(registrations);
    }
}
