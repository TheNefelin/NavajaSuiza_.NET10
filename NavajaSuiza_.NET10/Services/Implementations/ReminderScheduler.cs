using Microsoft.Maui.ApplicationModel;
using NavajaSuiza.Core.Interfaces;

namespace NavajaSuiza_.NET10.Services.Implementations;

public class ReminderScheduler : IReminderScheduler
{
    public const string ChannelId = "notes_reminders";
    internal const string ActionReminder = "com.nefelin.navajasuiza.REMINDER";

    public async Task<bool> RequestPermissionAsync()
    {
#if ANDROID
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Tiramisu)
            return true;

        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status == PermissionStatus.Granted)
            return true;

        return await Permissions.RequestAsync<Permissions.PostNotifications>() == PermissionStatus.Granted;
#elif IOS || MACCATALYST
        var center = UserNotifications.UNUserNotificationCenter.Current;
        var settings = await center.GetNotificationSettingsAsync();
        if (settings.AuthorizationStatus == UserNotifications.UNAuthorizationStatus.Authorized)
            return true;
        if (settings.AuthorizationStatus == UserNotifications.UNAuthorizationStatus.Denied)
            return false;

        var tcs = new TaskCompletionSource<bool>();
        center.RequestAuthorization(
            UserNotifications.UNAuthorizationOptions.Alert | UserNotifications.UNAuthorizationOptions.Sound,
            (granted, error) => tcs.TrySetResult(granted && error is null));
        return await tcs.Task;
#else
        return false; // Windows fuera de alcance: recordatorios no soportados
#endif
    }

    public Task ScheduleAsync(int noteId, string title, string body, DateTime localFireTime)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var alarmManager = (Android.App.AlarmManager?)context.GetSystemService(Android.Content.Context.AlarmService);
        if (alarmManager is null)
            return Task.CompletedTask;

        var intent = new Android.Content.Intent(context, typeof(NoteReminderReceiver))
            .SetAction(ActionReminder)
            .PutExtra(NoteReminderReceiver.ExtraNoteId, noteId)
            .PutExtra(NoteReminderReceiver.ExtraTitle, title)
            .PutExtra(NoteReminderReceiver.ExtraBody, body);

        var flags = Android.App.PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            flags |= Android.App.PendingIntentFlags.Immutable;
        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, noteId, intent, flags);
        if (pendingIntent is null)
            return Task.CompletedTask;

        var triggerAtMillis = (long)(localFireTime.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            alarmManager.SetAndAllowWhileIdle(Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        else
            alarmManager.Set(Android.App.AlarmType.RtcWakeup, triggerAtMillis, pendingIntent);
        return Task.CompletedTask;
#elif IOS || MACCATALYST
        var content = new UserNotifications.UNMutableNotificationContent
        {
            Title = title,
            Body = body,
            Sound = UserNotifications.UNNotificationSound.Default
        };

        var components = new Foundation.NSDateComponents
        {
            Year = localFireTime.Year,
            Month = localFireTime.Month,
            Day = localFireTime.Day,
            Hour = localFireTime.Hour,
            Minute = localFireTime.Minute,
            Second = 0
        };

        var trigger = UserNotifications.UNCalendarNotificationTrigger.CreateTrigger(components, false);
        var request = UserNotifications.UNNotificationRequest.FromIdentifier(
            noteId.ToString(System.Globalization.CultureInfo.InvariantCulture), content, trigger);
        return UserNotifications.UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
#else
        return Task.CompletedTask; // Windows fuera de alcance: recordatorios no soportados
#endif
    }

    public Task CancelAsync(int noteId)
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var alarmManager = (Android.App.AlarmManager?)context.GetSystemService(Android.Content.Context.AlarmService);
        if (alarmManager is null)
            return Task.CompletedTask;

        var intent = new Android.Content.Intent(context, typeof(NoteReminderReceiver)).SetAction(ActionReminder);
        var flags = Android.App.PendingIntentFlags.NoCreate;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            flags |= Android.App.PendingIntentFlags.Immutable;
        var pendingIntent = Android.App.PendingIntent.GetBroadcast(context, noteId, intent, flags);
        if (pendingIntent is not null)
            alarmManager.Cancel(pendingIntent);
        return Task.CompletedTask;
#elif IOS || MACCATALYST
        var id = noteId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        UserNotifications.UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new[] { id });
        return Task.CompletedTask;
#else
        return Task.CompletedTask; // Windows fuera de alcance: recordatorios no soportados
#endif
    }
}