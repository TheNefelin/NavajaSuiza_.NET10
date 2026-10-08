#if ANDROID
using Android.App;
using Android.Content;

namespace NavajaSuiza_.NET10.Services.Implementations;

[BroadcastReceiver(Enabled = true, Exported = false)]
[IntentFilter(new[] { ReminderScheduler.ActionReminder })]
public class NoteReminderReceiver : BroadcastReceiver
{
    public const string ExtraNotificationId = "notificationId";
    public const string ExtraTitle = "title";
    public const string ExtraBody = "body";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent?.Action != ReminderScheduler.ActionReminder)
            return;

        var notificationId = intent.GetIntExtra(ExtraNotificationId, 0);
        var title = intent.GetStringExtra(ExtraTitle) ?? string.Empty;
        var body = intent.GetStringExtra(ExtraBody) ?? string.Empty;

        var notificationManager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (notificationManager is null)
            return;

        CreateChannel(notificationManager);

        var launchIntent = new Intent(context, typeof(MainActivity))
            .SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingFlags = PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
            pendingFlags |= PendingIntentFlags.Immutable;
        var contentIntent = PendingIntent.GetActivity(context, 0, launchIntent, pendingFlags);

        Notification.Builder builder;
        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            builder = new Notification.Builder(context, ReminderScheduler.ChannelId);
        }
        else
        {
#pragma warning disable CA1422 // Fallback sin canal para Android < 8 (API < 26)
            builder = new Notification.Builder(context);
#pragma warning restore CA1422
        }

        builder.SetSmallIcon(Resource.Drawable.ic_stat_bell)
            .SetContentTitle(string.IsNullOrWhiteSpace(title) ? "Nota" : title)
            .SetContentText(body)
            .SetAutoCancel(true);

        if (contentIntent is not null)
            builder.SetContentIntent(contentIntent);

        notificationManager.Notify(notificationId, builder.Build());
    }

    private static void CreateChannel(NotificationManager manager)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var channel = new NotificationChannel(ReminderScheduler.ChannelId, "Navaja Suiza", NotificationImportance.Default);
        manager.CreateNotificationChannel(channel);
    }
}
#endif