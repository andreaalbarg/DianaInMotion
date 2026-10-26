using dianainmotion.Backend.Models;
using dianainmotion.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/email-test")]
[Authorize(Roles = "Admin")]
public class AdminEmailTestController(IEmailService emailService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] EmailTestRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.To) || !req.To.Contains('@'))
            return BadRequest(new ProblemDetails { Title = "Ingresa un correo válido." });

        if (!emailService.IsConfigured())
            return BadRequest(new ProblemDetails
            {
                Title = "SMTP no configurado. Añade las variables Smtp__Host, Smtp__Username y Smtp__Password en el entorno del backend."
            });

        var language = req.Language == "en" ? "en" : "es";
        var fakeDate = DateTime.UtcNow.AddDays(7);
        const string fakeTitle = "Getting Started with Diana in Motion";
        const string fakeMeeting = "https://zoom.us/j/123456789";

        await emailService.SendConfirmationAsync(req.To, req.Name ?? req.To, fakeTitle, fakeDate, fakeMeeting, language, ct);

        return Ok(new { message = $"Correo de prueba enviado a {req.To}. Revisa tu bandeja de entrada (y spam)." });
    }
}

public record EmailTestRequest
{
    public required string To { get; init; }
    public string? Name { get; init; }
    public string Language { get; init; } = "es";
}
