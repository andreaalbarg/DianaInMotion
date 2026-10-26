using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace dianainmotion.Backend.Services;

public interface IEmailService
{
    Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, CancellationToken ct);
    Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, CancellationToken ct);
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

    public async Task SendConfirmationAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, CancellationToken ct)
    {
        var cfg = GetConfig();
        if (cfg is null)
        {
            logger.LogWarning("SMTP not configured. Skipping confirmation email to {Email}.", toEmail);
            return;
        }
        var subject = $"¡Registro confirmado! – {eventTitle}";
        var body = BuildConfirmationBody(toName, eventTitle, scheduledAt, meetingUrl);
        await SendAsync(cfg, toEmail, toName, subject, body, ct);
    }

    public async Task SendReminderAsync(string toEmail, string toName, string eventTitle, DateTime scheduledAt, string? meetingUrl, CancellationToken ct)
    {
        var cfg = GetConfig();
        if (cfg is null)
        {
            logger.LogWarning("SMTP not configured. Skipping reminder email to {Email}.", toEmail);
            return;
        }
        var subject = $"¡Recuerda: mañana es {eventTitle}!";
        var body = BuildReminderBody(toName, eventTitle, scheduledAt, meetingUrl);
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

    private static string FormatDate(DateTime dt)
    {
        string[] months = ["", "enero", "febrero", "marzo", "abril", "mayo", "junio",
            "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];
        return $"{dt.Day} de {months[dt.Month]} de {dt.Year} a las {dt.Hour:D2}:{dt.Minute:D2} UTC";
    }

    private static string MeetingSection(string? meetingUrl)
    {
        if (string.IsNullOrWhiteSpace(meetingUrl))
            return "<p style='margin:4px 0;color:#666;'>El enlace de acceso estará disponible próximamente.</p>";
        var encoded = System.Net.WebUtility.HtmlEncode(meetingUrl);
        return $"<p style='margin:4px 0;'><strong>Liga de acceso:</strong> <a href='{encoded}' style='color:#7abf1e;word-break:break-all;'>{encoded}</a></p>";
    }

    private static string BuildConfirmationBody(string name, string eventTitle, DateTime scheduledAt, string? meetingUrl) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:32px 16px;color:#231f20;">
          <div style="background:#c2f33d;padding:24px 32px;border-radius:12px 12px 0 0;">
            <h1 style="margin:0;font-size:22px;color:#231f20;">Diana in Motion</h1>
          </div>
          <div style="border:1px solid #e5e7eb;border-top:none;padding:32px;border-radius:0 0 12px 12px;">
            <h2 style="color:#231f20;">¡Hola, {System.Net.WebUtility.HtmlEncode(name)}!</h2>
            <p>Tu registro para <strong>{System.Net.WebUtility.HtmlEncode(eventTitle)}</strong> ha sido confirmado. ¡Te esperamos!</p>
            <div style="background:#f9fafb;border-left:4px solid #c2f33d;padding:16px 20px;margin:24px 0;border-radius:0 8px 8px 0;">
              <p style="margin:0 0 8px;"><strong>Evento:</strong> {System.Net.WebUtility.HtmlEncode(eventTitle)}</p>
              <p style="margin:0 0 8px;"><strong>Fecha:</strong> {FormatDate(scheduledAt)}</p>
              {MeetingSection(meetingUrl)}
            </div>
            <p style="color:#666;font-size:14px;">Si tienes alguna pregunta, responde a este correo.</p>
            <p style="margin-top:32px;">¡Nos vemos ahí!<br/><strong>— El equipo de Diana in Motion</strong></p>
          </div>
        </body>
        </html>
        """;

    private static string BuildReminderBody(string name, string eventTitle, DateTime scheduledAt, string? meetingUrl) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:sans-serif;max-width:600px;margin:0 auto;padding:32px 16px;color:#231f20;">
          <div style="background:#c2f33d;padding:24px 32px;border-radius:12px 12px 0 0;">
            <h1 style="margin:0;font-size:22px;color:#231f20;">Diana in Motion</h1>
          </div>
          <div style="border:1px solid #e5e7eb;border-top:none;padding:32px;border-radius:0 0 12px 12px;">
            <h2 style="color:#231f20;">¡Hola, {System.Net.WebUtility.HtmlEncode(name)}!</h2>
            <p>Te recordamos que <strong>mañana</strong> es el evento al que te registraste:</p>
            <div style="background:#f9fafb;border-left:4px solid #c2f33d;padding:16px 20px;margin:24px 0;border-radius:0 8px 8px 0;">
              <p style="margin:0 0 8px;"><strong>Evento:</strong> {System.Net.WebUtility.HtmlEncode(eventTitle)}</p>
              <p style="margin:0 0 8px;"><strong>Fecha:</strong> {FormatDate(scheduledAt)}</p>
              {MeetingSection(meetingUrl)}
            </div>
            <p style="margin-top:32px;">¡Nos vemos mañana!<br/><strong>— El equipo de Diana in Motion</strong></p>
          </div>
        </body>
        </html>
        """;
}
