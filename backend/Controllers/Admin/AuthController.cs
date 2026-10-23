using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dianainmotion.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace dianainmotion.Backend.Controllers.Admin;

[ApiController]
[Route("api/admin/auth")]
[AllowAnonymous]
public class AuthController(AdminSettings adminSettings, JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (!BCrypt.Net.BCrypt.Verify(req.Password, adminSettings.PasswordHash))
            return Unauthorized(new ProblemDetails { Title = "Invalid password." });

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.Role, "Admin")],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return Ok(new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
