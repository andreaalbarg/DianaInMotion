using dianainmotion.Backend.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace dianainmotion.Backend.Services;

public class ReminderBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await SendPendingRemindersAsync(ct); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { logger.LogError(ex, "Error sending reminder emails."); }

            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }

    private async Task SendPendingRemindersAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddHours(20);
        var windowEnd = now.AddHours(28);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var pending = await db.Registrations
            .Include(r => r.Event)
            .Where(r => r.ReminderSentAt == null
                     && r.Event.ScheduledAt >= windowStart
                     && r.Event.ScheduledAt <= windowEnd)
            .ToListAsync(ct);

        if (pending.Count == 0) return;

        logger.LogInformation("Sending {Count} reminder emails.", pending.Count);

        foreach (var reg in pending)
        {
            await emailService.SendReminderAsync(
                reg.Email, reg.Name,
                reg.Event.Title, reg.Event.ScheduledAt, reg.Event.MeetingUrl, reg.Event.Language, ct);
            reg.ReminderSentAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
