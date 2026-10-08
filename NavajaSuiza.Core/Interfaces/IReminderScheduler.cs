using NavajaSuiza.Core.Models;

namespace NavajaSuiza.Core.Interfaces;

public interface IReminderScheduler
{
    Task<bool> RequestPermissionAsync();

    Task ScheduleAsync(ReminderKind kind, int id, string title, string body, DateTime localFireTime);

    Task CancelAsync(ReminderKind kind, int id);
}