namespace NavajaSuiza.Core.Interfaces;

public interface IReminderScheduler
{
    Task<bool> RequestPermissionAsync();

    Task ScheduleAsync(int noteId, string title, string body, DateTime localFireTime);

    Task CancelAsync(int noteId);
}