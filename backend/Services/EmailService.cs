using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace dianainmotion.Backend.Services;

public interface IEmailService
{
    Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct);
    Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct);
}

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private record SmtpConfig(string Host, int Port, string Username, string Password, string FromEmail, string FromName);

    private SmtpConfig? GetConfig()
    {
        var host = config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host)) return null;
        return new SmtpConfig(
            host,
            int.TryParse(config["Smtp:Port"], out var port) ? port : 587,
            config["Smtp:Username"] ?? "",
            config["Smtp:Password"] ?? "",
            config["Smtp:FromEmail"] is { Length: > 0 } fe ? fe : config["Smtp:Username"] ?? "",
            config["Smtp:FromName"] is { Length: > 0 } fn ? fn : "Diana in Motion"
        );
    }

    public async Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct)
    {
        var cfg = GetConfig();
        if (cfg is null)
        {
            logger.LogWarning("SMTP not configured. Skipping confirmation email to {Email}.", toEmail);
            return;
        }
        var es = language != "en";
        var subject = es ? $"¡Registro confirmado! – {eventTitle}" : $"Registration confirmed! – {eventTitle}";
        var body = BuildConfirmationBody(toName, eventTitle, scheduledAt, meetingUrl, es);
        await SendAsync(cfg, toEmail, toName, subject, body, ct);
    }

    public async Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, string language, CancellationToken ct)
    {
        var cfg = GetConfig();
        if (cfg is null)
        {
            logger.LogWarning("SMTP not configured. Skipping reminder email to {Email}.", toEmail);
            return;
        }
        var es = language != "en";
        var subject = es ? $"¡Recuerda: mañana es {eventTitle}!" : $"Reminder: {eventTitle} is tomorrow!";
        var body = BuildReminderBody(toName, eventTitle, scheduledAt, meetingUrl, es);
        await SendAsync(cfg, toEmail, toName, subject, body, ct);
    }

    private async Task SendAsync(SmtpConfig cfg, string toEmail, string toName, string subject, string htmlBody, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(cfg.FromName, cfg.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(cfg.Host, cfg.Port, SecureSocketOptions.Auto, ct);
            if (!string.IsNullOrEmpty(cfg.Username))
                await client.AuthenticateAsync(cfg.Username, cfg.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
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
        var team        = es ? "El equipo de Diana in Motion" : "The Diana in Motion team";

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
        var team        = es ? "El equipo de Diana in Motion" : "The Diana in Motion team";

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
