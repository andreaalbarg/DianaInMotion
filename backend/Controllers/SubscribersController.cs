using dianainmotion.Backend.Infrastructure;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SubscribersController(AppDb db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest(new ProblemDetails { Title = "Invalid email address." });

        var exists = await db.Subscribers.AnyAsync(s => s.Email == email, ct);
        if (exists)
            return Ok(new { message = "You're already subscribed." });

        db.Subscribers.Add(new Subscriber { Id = Guid.NewGuid(), Email = email });
        await db.SaveChangesAsync(ct);

        return Ok(new { message = "Subscribed successfully." });
    }
}
