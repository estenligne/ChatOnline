using Xamarin.Forms;
using XamApp.Models;
using XamApp.Views;
using XamApp.Services;
using XamApp.ViewModels;
using Global.Enums;
using Global.Models;
using System;
using System.Web;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace XamApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = GetMainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private static PushNotificationDTO Notification = null;

        private static Page GetMainPage()
        {
            var user = DataStore.Instance.GetUserAsync().Result;
            string page = nameof(RoomsPage);

            if (user == null)
            {
                page = nameof(SignInPage);
            }
            else if (Notification != null)
            {
                long? chatRoomId = Notification.MessageSent?.MessageTag?.ChatRoomId;
                if (chatRoomId.HasValue)
                {
                    page += "/" + nameof(ChatRoomPage) + "?id=" + chatRoomId;
                }
                Notification = null;
            }

            var appShell = new AppShell();
            appShell.GoToAsync("//" + page).Wait();
            return appShell;
        }

        public enum NotificationSource
        {
            OnLaunch,
            OnResume,
            OnBackground,
            OnForeground,
        }

        public static void OnNotificationReceived(string data, NotificationSource source)
        {
            var notification = JsonConvert.DeserializeObject<PushNotificationDTO>(data);

            if (source == NotificationSource.OnLaunch)
                Notification = notification;

            else if (source == NotificationSource.OnBackground)
                ProcessNotificationReceived(notification, source);

            else Device.BeginInvokeOnMainThread(() => ProcessNotificationReceived(notification, source));
        }

        private static async void ProcessNotificationReceived(PushNotificationDTO notification, NotificationSource source)
        {
            string error = null;
            try
            {
                if (source == NotificationSource.OnResume)
                {
                    long? chatRoomId = notification.MessageSent?.MessageTag?.ChatRoomId;

                    if (chatRoomId.HasValue)
                    {
                        await Shell.Current.GoToAsync(nameof(ChatRoomPage) + "?id=" + chatRoomId);
                    }
                }
                else
                {
                    if (notification.Topic == PushNotificationTopic.MessageSent)
                    {
                        string dateReceived = HttpUtility.UrlEncode(DateTimeOffset.Now.ToString("O"));
                        string args = $"?messageSentId={notification.MessageSent.Id}&dateReceived={dateReceived}";

                        var response = await HTTPClient.PostAsync<string>(null, "/api/Message/Received" + args, null);
                        if (response.IsSuccessStatusCode)
                        {
                            var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);

                            Trace.TraceInformation($"ProcessNotificationReceived() ReceiverId: {message.ReceiverId}");

                            if (source == NotificationSource.OnForeground &&
                                ChatRoomViewModel.AddMessageFromPushNotification(message))
                            {
                                Vibrate(500);
                            }
                            else
                            {
                                DependencyService.Get<INotifications>().Notify(notification);
                            }
                        }
                        else error = await HTTPClient.GetResponseError(response);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            if (error != null)
            {
                Trace.TraceError($"ProcessNotificationReceived() Error: {error}");
                Vibrate(1000);
            }
        }

        public static void Vibrate(double milliseconds)
        {
            try
            {
                // see https://docs.microsoft.com/en-us/xamarin/essentials/vibrate
                Xamarin.Essentials.Vibration.Vibrate(milliseconds);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Vibrate() Error: {ex.Message}");
            }
        }

        public static void Toast(string message, bool takeLong)
        {
            DependencyService.Get<INotifications>().Toast(message, takeLong);
        }

        public static DevicePlatformEnum DevicePlatform()
        {
            var platform =
                Device.RuntimePlatform == Device.Android ? DevicePlatformEnum.Android :
                Device.RuntimePlatform == Device.iOS ? DevicePlatformEnum.AppleiOS :
                DevicePlatformEnum.Unknown;
            return platform;
        }
    }
}
