using Android.App;
using Android.Content;
using Android.Media;
using Firebase.Messaging;
using System;
using Global.Models;

namespace XamApp.Droid.Notifications
{
    [Service(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        // https://firebase.google.com/docs/reference/android/com/google/firebase/messaging/FirebaseMessagingService
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            Notifications.RegisterFcmToken(token);
        }

        // IMPORTANT: https://firebase.google.com/docs/cloud-messaging/android/receive
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            int play = 0;
            try
            {
                if (message.Data.ContainsKey(PushNotificationDTO.Key))
                {
                    var source = AppIsInForeground() ?
                        App.NotificationSource.OnForeground :
                        App.NotificationSource.OnBackground;

                    App.Notifications = new Notifications();
                    string data = message.Data[PushNotificationDTO.Key];
                    App.OnNotificationReceived(data, source);
                }
                else play = 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"OnMessageReceived() Error: {ex}");
                play = 2;
            }
            if (play != 0) // inform that a push notification was received
            {
                var soundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                var sound = RingtoneManager.GetRingtone(ApplicationContext, soundUri);
                sound.Play();
            }
        }

        private static bool AppIsInForeground()
        {
            ActivityManager.RunningAppProcessInfo myProcess = new ActivityManager.RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(myProcess);
            return myProcess.Importance == Importance.Foreground;
        }
    }
}
