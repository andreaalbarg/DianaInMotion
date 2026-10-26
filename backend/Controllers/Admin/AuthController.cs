using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dianainmotion.Backend.Models;
using dianainmotion.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
[AllowAnonymous]
public class AuthController(
    AdminSettings adminSettings,
    JwtSettings jwtSettings,
    AdminResetTokenStore tokenStore,
    IEmailService emailService,
    IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        // Same message for wrong email or wrong password — don't reveal which
        var ok = string.Equals(req.Email.Trim(), adminSettings.Email, StringComparison.OrdinalIgnoreCase)
               && BCrypt.Net.BCrypt.Verify(req.Password, adminSettings.PasswordHash);

        if (!ok) return Unauthorized(new ProblemDetails { Title = "Correo o contraseña incorrectos." });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Role, "Admin")],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        // Always return same response to avoid revealing whether email exists
        if (string.Equals(req.Email.Trim(), adminSettings.Email, StringComparison.OrdinalIgnoreCase)
            && emailService.IsConfigured())
        {
            var rawToken = tokenStore.Generate();
            // Use the Origin header (sent by browser) so the link points to the frontend, not the backend
            var origin = Request.Headers.Origin.ToString();
            var referer = Request.Headers.Referer.ToString();
            var frontendBase = config["FrontendUrl"] is { Length: > 0 } fb
                ? fb
                : !string.IsNullOrEmpty(origin) ? origin
                : !string.IsNullOrEmpty(referer) ? new Uri(referer).GetLeftPart(UriPartial.Authority)
                : $"{Request.Scheme}://{Request.Host}";
            var link = $"{frontendBase}/admin/reset-password?token={Uri.EscapeDataString(rawToken)}";
            await emailService.SendPasswordResetAsync(adminSettings.Email, link, ct);
        }
        return Ok(new { message = "Si ese correo está registrado, recibirás un enlace en los próximos minutos." });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (!tokenStore.Validate(req.Token))
            return BadRequest(new ProblemDetails { Title = "El enlace expiró o ya fue usado. Solicita uno nuevo." });

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest(new ProblemDetails { Title = "La contraseña debe tener al menos 8 caracteres." });

        var newHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        adminSettings = adminSettings with { PasswordHash = newHash };
        tokenStore.Consume();

        // Persist new password so it survives restarts
        var dataDir = Environment.GetEnvironmentVariable("DIANA_DATA_DIR")
                      ?? System.IO.Path.GetDirectoryName(typeof(AuthController).Assembly.Location)!;
        System.IO.File.WriteAllText(System.IO.Path.Combine(dataDir, "admin_password.txt"), req.NewPassword);

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }
}
