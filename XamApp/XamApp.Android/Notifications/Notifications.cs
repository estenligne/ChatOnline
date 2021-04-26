﻿using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Gms.Tasks;
using Firebase.Messaging;
using Global.Enums;
using Global.Models;
using Newtonsoft.Json;
using XamApp.Services;

[assembly: Xamarin.Forms.Dependency(typeof(XamApp.Droid.Notifications.Notifications))]
namespace XamApp.Droid.Notifications
{
    public class Notifications : Java.Lang.Object, IOnCompleteListener, INotifications
    {
        public const string Key = nameof(PushNotificationDTO);

        public static long DeviceUsedId = 0;

        public static async void RegisterFcmToken(string fcmToken)
        {
            await HTTPClient.RegisterFcmToken(DeviceUsedId, fcmToken);
        }

        void IOnCompleteListener.OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                string fcmToken = task.Result.ToString();
                System.Threading.Tasks.Task.Run(() => RegisterFcmToken(fcmToken));
            }
        }

        void INotifications.RegisterFcmToken(long deviceUsedId)
        {
            DeviceUsedId = deviceUsedId;
            var task = FirebaseMessaging.Instance.GetToken();
            task.AddOnCompleteListener(this);
        }

        void INotifications.Notify(PushNotificationDTO notification)
        {
            Context context = Application.Context;

            var channelId = PushNotificationDTO.GetChannelId(notification.Topic);

            var notificationBuilder = new NotificationCompat.Builder(context, channelId)
                .SetSmallIcon(Resource.Drawable.icon_about)
                .SetColor(ContextCompat.GetColor(context, Resource.Color.colorPrimary))
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Body)
                .SetAutoCancel(true);

            var extras = new Bundle();
            extras.PutString(Key, JsonConvert.SerializeObject(notification));

            var resultIntent = new Intent(context, typeof(MainActivity));
            resultIntent.PutExtras(extras);
            resultIntent.SetFlags(ActivityFlags.SingleTop);

            var resultPendingIntent = PendingIntent.GetActivity(context, (int)notification.Topic, resultIntent, PendingIntentFlags.OneShot);
            notificationBuilder.SetContentIntent(resultPendingIntent);

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(notification.Tag, notification.Id, notificationBuilder.Build());
        }

        public static void ProcessIntent(Intent intent, App.NotificationSource source)
        {
            if (intent.HasExtra(Key))
            {
                var data = intent.GetStringExtra(Key);
                var notification = JsonConvert.DeserializeObject<PushNotificationDTO>(data);
                App.OnNotificationReceived(notification, source);
            }
        }

        public static void Setup(ContextWrapper context)
        {
            // Setup Notification Channels if supported by the Anrdoid OS
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string channelId = PushNotificationDTO.GetChannelId(PushNotificationTopic.MessageSent);
                var nc = new NotificationChannel(channelId, "Chat Message", NotificationImportance.High);

                nc.LockscreenVisibility = NotificationVisibility.Public;
                nc.EnableVibration(true);
                nc.EnableLights(true);

                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(nc);

                channelId = PushNotificationDTO.GetChannelId(PushNotificationTopic.None);
                nc = new NotificationChannel(channelId, "Default", NotificationImportance.Default);
                nc.LockscreenVisibility = NotificationVisibility.Public;
                nc.EnableLights(true);
                notificationManager.CreateNotificationChannel(nc);
            }
        }
    }
}