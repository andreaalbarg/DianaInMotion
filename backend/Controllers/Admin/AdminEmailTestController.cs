using dianainmotion.Backend.Models;
using dianainmotion.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/email-test")]
[Authorize(Roles = "Admin")]
public class AdminEmailTestController(IEmailService emailService, IConfiguration config) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] EmailTestRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.To) || !req.To.Contains('@'))
            return BadRequest(new ProblemDetails { Title = "Ingresa un correo válido." });

        if (!emailService.IsConfigured())
        {
            var detected = config["Smtp:Host"];
            var msg = string.IsNullOrWhiteSpace(detected)
                ? "SMTP no configurado. El app no detecta ningún valor en Smtp__Host."
                : $"SMTP no configurado correctamente. Smtp__Host detectado: '{detected}' — verifica que sea un host válido.";
            return BadRequest(new ProblemDetails { Title = msg });
        }

        var language = req.Language == "en" ? "en" : "es";
        var fakeDate = DateTime.UtcNow.AddDays(7);
        const string fakeTitle = "Getting Started with Diana in Motion";
        const string fakeMeeting = "https://zoom.us/j/123456789";

        // Fire-and-forget — SMTP puede tardar varios segundos en conectar
        _ = emailService.SendConfirmationAsync(req.To, req.Name ?? req.To, fakeTitle, fakeDate, fakeMeeting, language, CancellationToken.None);

        return Ok(new { message = $"Solicitud enviada a {req.To}. Revisa tu bandeja en 1-2 minutos (incluyendo spam)." });
    }
}

public record EmailTestRequest
{
    public required string To { get; init; }
    public string? Name { get; init; }
    public string Language { get; init; } = "es";
}
