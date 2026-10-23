using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/subscribers")]
[Authorize(Roles = "Admin")]
public class AdminSubscribersController(AppDb db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var subscribers = await db.Subscribers
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriberDto
            {
                Id = s.Id,
                Email = s.Email,
                SubscribedAt = s.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(subscribers);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var sub = await db.Subscribers.FindAsync([id], ct);
        if (sub is null) return NotFound();
        db.Subscribers.Remove(sub);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
