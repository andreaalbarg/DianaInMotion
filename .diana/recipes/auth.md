# Secure auth recipe (.NET) — read before adding login / accounts / roles

This project was scaffolded WITHOUT auth. When the user asks for login, registration,
accounts, or roles, base your implementation on this pattern. It satisfies Diana's
security baseline — the ship-time security gate checks the same rules, so deviating can
BLOCK the deploy. Adapt names/fields to the domain; keep the security properties.

## NuGet
- Microsoft.AspNetCore.Authentication.JwtBearer
- BCrypt.Net-Next

## 1. Secret from environment — never committed
In Program.cs (fail closed if missing):

    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret not configured");
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,  ValidIssuer = "app",
            ValidateAudience = true, ValidAudience = "app",
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        });
    builder.Services.AddAuthorization();
    // pipeline: app.UseAuthentication(); app.UseAuthorization();

appsettings.json keeps an EMPTY placeholder: "Jwt": { "Secret": "" }. The real value comes
from env var Jwt__Secret. NEVER commit a usable secret (the gate blocks it).

## 2. Registration NEVER trusts a client-supplied role
The request has NO Role field; the server forces the lowest role:

    public record RegisterRequest(string Email, string FullName, string Password);
    // ...
    var user = new User {
        Email = req.Email.ToLowerInvariant(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        FullName = req.FullName,
        Role = "User"   // server-decided. Elevation is a separate admin-only endpoint.
    };

## 3. Read the acting user from the token, never from the body

    private Guid UserId => Guid.Parse(
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

## 4. Password reset — token goes out of band, NEVER in the response

    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequest req) {
        var token = await _auth.CreateResetTokenAsync(req.Email); // store HASHED + short expiry
        if (token is not null) await _mail.SendResetAsync(req.Email, token); // no-op in dev is fine
        return Ok(new { message = "If that email exists, a reset link has been sent." });
        // NEVER return the token, never log it. Same message whether or not the email exists.
    }

## 5. Default-deny + own-resource scoping
- [Authorize] on controllers; [AllowAnonymous] only on login/register/public reads (comment why).
- Role-gate with [Authorize(Roles = "Admin")] using a Roles constants class.
- Every query for a user-owned row filters by the token user:
      _db.Things.Where(t => t.Id == id && t.UserId == UserId)   // never FindAsync(id) alone

## 6. CORS — explicit allowlist from config, never AllowAnyOrigin / SetIsOriginAllowed(_ => true).

## 7. Passwords & tokens — BCrypt/argon2; short-lived access tokens; refresh for long sessions.

## Seed admin
If you seed an admin, generate a RANDOM password and surface it once — never a known
default like "Admin@123".