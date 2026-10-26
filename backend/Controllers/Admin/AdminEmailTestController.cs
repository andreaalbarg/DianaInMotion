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
            return BadRequest(new ProblemDetails { Title = "Resend no configurado. Añade la variable Resend__ApiKey al entorno del backend." });

        var language = req.Language == "en" ? "en" : "es";
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeout.Token);
        try
        {
            await emailService.SendTestAsync(req.To, req.Name ?? req.To, language, linked.Token);
            return Ok(new { message = $"✓ Correo enviado a {req.To}. Revisa tu bandeja (incluyendo spam)." });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(504, new ProblemDetails { Title = "La API de Resend no respondió en 30 segundos." });
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails { Title = $"Error Resend: {ex.Message}" });
        }
    }
}

public record EmailTestRequest
{
    public required string To { get; init; }
    public string? Name { get; init; }
    public string Language { get; init; } = "es";
}
