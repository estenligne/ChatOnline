using Android.App;
using Android.Content;
using Android.Media;
using Firebase.Messaging;
using System;
using Global.Models;
using Newtonsoft.Json;

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
            try
            {
                if (message.Data.ContainsKey(Notifications.Key))
                {
                    var source = AppIsInForeground() ?
                        App.NotificationSource.OnForeground :
                        App.NotificationSource.OnBackground;

                    var data = message.Data[Notifications.Key];
                    var notification = JsonConvert.DeserializeObject<PushNotificationDTO>(data);
                    App.OnNotificationReceived(notification, source);
                }
                else
                {
                    var soundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                    var sound = RingtoneManager.GetRingtone(ApplicationContext, soundUri);
                    sound.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnMessageReceived() Error: {ex}");
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
