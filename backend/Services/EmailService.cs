using Resend;

namespace dianainmotion.Backend.Services;

public interface IEmailService
{
    bool IsConfigured();
    Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct);
    Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct);
    /// <summary>Sends a test email and throws on failure so the caller can surface the error.</summary>
    Task SendTestAsync(string toEmail, string toName, string language, CancellationToken ct);
}

public class EmailService(IResend resend, IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private string FromAddress => config["Resend:From"] is { Length: > 0 } f ? f : "Diana in Motion <onboarding@resend.dev>";

    public bool IsConfigured() => !string.IsNullOrWhiteSpace(config["Resend:ApiKey"]);

    public async Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct)
    {
        if (!IsConfigured()) { logger.LogWarning("Resend not configured. Skipping confirmation email to {Email}.", toEmail); return; }
        var es = language != "en";
        await SendAsync(toEmail, es ? $"¡Registro confirmado! – {eventTitle}" : $"Registration confirmed! – {eventTitle}",
            BuildConfirmationBody(toName, eventTitle, scheduledAt, meetingUrl, es), ct);
    }

    public async Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct)
    {
        if (!IsConfigured()) { logger.LogWarning("Resend not configured. Skipping reminder email to {Email}.", toEmail); return; }
        var es = language != "en";
        await SendAsync(toEmail, es ? $"¡Recuerda: mañana es {eventTitle}!" : $"Reminder: {eventTitle} is tomorrow!",
            BuildReminderBody(toName, eventTitle, scheduledAt, meetingUrl, es), ct);
    }

    public async Task SendTestAsync(string toEmail, string toName, string language, CancellationToken ct)
    {
        if (!IsConfigured()) throw new InvalidOperationException("Resend no configurado: añade la variable Resend__ApiKey al backend.");
        var es = language != "en";
        await SendAsync(toEmail,
            es ? "Correo de prueba – Diana in Motion" : "Test email – Diana in Motion",
            BuildConfirmationBody(toName, "Getting Started with Diana in Motion", DateTime.UtcNow.AddDays(7), "https://zoom.us/j/123456789", es),
            ct);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var msg = new EmailMessage { From = FromAddress, Subject = subject, HtmlBody = htmlBody };
        msg.To.Add(toEmail);
        try
        {
            await resend.EmailSendAsync(msg, ct);
            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }

    private static string FormatDate(DateTime dt, bool es)
    {
        if (es)
        {
            string[] months = ["", "enero", "febrero", "marzo", "abril", "mayo", "junio",
                "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];
            return $"{dt.Day} de {months[dt.Month]} de {dt.Year} a las {dt.Hour:D2}:{dt.Minute:D2} UTC";
        }
        return dt.ToString("MMMM d, yyyy") + $" at {dt.Hour:D2}:{dt.Minute:D2} UTC";
    }

    private static string MeetingSection(string? meetingUrl, bool es)
    {
        if (string.IsNullOrWhiteSpace(meetingUrl))
        {
            var msg = es ? "El enlace de acceso estará disponible próximamente."
                        : "The access link will be available soon.";
            return $"<p style='margin:4px 0;color:#666;'>{msg}</p>";
        }
        var label = es ? "Liga de acceso" : "Join link";
        var encoded = System.Net.WebUtility.HtmlEncode(meetingUrl);
        return $"<p style='margin:4px 0;'><strong>{label}:</strong> <a href='{encoded}' style='color:#c2f33d;word-break:break-all;'>{encoded}</a></p>";
    }

    private static string BuildConfirmationBody(string name, string eventTitle, DateTime scheduledAt, string? meetingUrl, bool es)
    {
        var greeting    = es ? $"¡Hola, {H(name)}!" : $"Hi, {H(name)}!";
        var intro       = es ? $"Tu registro para <strong>{H(eventTitle)}</strong> ha sido confirmado. ¡Te esperamos!"
                             : $"Your registration for <strong>{H(eventTitle)}</strong> has been confirmed. We look forward to seeing you!";
        var labelEvento = es ? "Evento" : "Event";
        var labelFecha  = es ? "Fecha"  : "Date";
        var footer      = es ? "Si tienes alguna pregunta, responde a este correo."
                             : "If you have any questions, reply to this email.";
        var closing     = es ? "¡Nos vemos ahí!" : "See you there!";
        var team        = "Diana's Team";

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:32px 16px;color:#231f20;">
              <div style="background:#231f20;padding:24px 32px;border-radius:12px 12px 0 0;">
                <h1 style="margin:0;font-size:22px;color:#ffffff;">Diana in Motion</h1>
              </div>
              <div style="border:1px solid #e5e7eb;border-top:none;padding:32px;border-radius:0 0 12px 12px;">
                <h2 style="color:#231f20;">{greeting}</h2>
                <p>{intro}</p>
                <div style="background:#f9fafb;border-left:4px solid #231f20;padding:16px 20px;margin:24px 0;border-radius:0 8px 8px 0;">
                  <p style="margin:0 0 8px;"><strong>{labelEvento}:</strong> {H(eventTitle)}</p>
                  <p style="margin:0 0 8px;"><strong>{labelFecha}:</strong> {FormatDate(scheduledAt, es)}</p>
                  {MeetingSection(meetingUrl, es)}
                </div>
                <p style="color:#666;font-size:14px;">{footer}</p>
                <p style="margin-top:32px;">{closing}<br/><strong>— {team}</strong></p>
              </div>
            </body>
            </html>
            """;
    }

    private static string BuildReminderBody(string name, string eventTitle, DateTime scheduledAt, string? meetingUrl, bool es)
    {
        var greeting    = es ? $"¡Hola, {H(name)}!" : $"Hi, {H(name)}!";
        var intro       = es ? $"Te recordamos que <strong>mañana</strong> es el evento al que te registraste:"
                             : $"This is a reminder that <strong>tomorrow</strong> is the event you registered for:";
        var labelEvento = es ? "Evento" : "Event";
        var labelFecha  = es ? "Fecha"  : "Date";
        var closing     = es ? "¡Nos vemos mañana!" : "See you tomorrow!";
        var team        = "Diana's Team";

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:32px 16px;color:#231f20;">
              <div style="background:#231f20;padding:24px 32px;border-radius:12px 12px 0 0;">
                <h1 style="margin:0;font-size:22px;color:#ffffff;">Diana in Motion</h1>
              </div>
              <div style="border:1px solid #e5e7eb;border-top:none;padding:32px;border-radius:0 0 12px 12px;">
                <h2 style="color:#231f20;">{greeting}</h2>
                <p>{intro}</p>
                <div style="background:#f9fafb;border-left:4px solid #231f20;padding:16px 20px;margin:24px 0;border-radius:0 8px 8px 0;">
                  <p style="margin:0 0 8px;"><strong>{labelEvento}:</strong> {H(eventTitle)}</p>
                  <p style="margin:0 0 8px;"><strong>{labelFecha}:</strong> {FormatDate(scheduledAt, es)}</p>
                  {MeetingSection(meetingUrl, es)}
                </div>
                <p style="margin-top:32px;">{closing}<br/><strong>— {team}</strong></p>
              </div>
            </body>
            </html>
            """;
    }

    private static string H(string s) => System.Net.WebUtility.HtmlEncode(s);
}
