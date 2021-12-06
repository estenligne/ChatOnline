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
                Notification = null;

                if (chatRoomId.HasValue)
                {
                    string error = Task.Run(async () => await SetChatRoomPage(chatRoomId.Value)).Result;

                    if (error == null)
                        page += "/" + nameof(ChatRoomPage);
                }
            }

            var appShell = new AppShell();
            appShell.GoToAsync("//" + page).Wait();
            return appShell;
        }

        private static async Task<string> SetChatRoomPage(long chatRoomId)
        {
            string url = "/api/ChatRoom/GetInfo/" + chatRoomId;
            var response = await HTTPClient.GetAsync(null, url);

            if (response.IsSuccessStatusCode)
            {
                ChatRoomViewModel.Room = await HTTPClient.ReadAsAsync<RoomInfo>(response);
                return null;
            }
            else return await HTTPClient.GetResponseError(response);
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
                        error = await SetChatRoomPage(chatRoomId.Value);

                        if (error == null)
                            await Shell.Current.GoToAsync(nameof(ChatRoomPage));
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
