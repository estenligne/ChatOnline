using Xamarin.Forms;
using XamApp.Models;
using XamApp.Views;
using XamApp.Services;
using XamApp.ViewModels;
using Global.Enums;
using Global.Models;
using System;
using System.Threading.Tasks;

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
                page = nameof(LoginPage);
            }
            else if (Notification != null)
            {
                var success = Task.Run(async () => await SetChatRoomPage(Notification.UserChatRoomId)).Result;
                if (success)
                    page += "/" + nameof(ChatRoomPage);
                Notification = null;
            }

            var appShell = new AppShell();
            appShell.GoToAsync("//" + page).Wait();
            return appShell;
        }

        private static async Task<bool> SetChatRoomPage(long userChatRoomId)
        {
            var url = $"/api/ChatRoom/GetInfo?userChatRoomId={userChatRoomId}";
            var response = await HTTPClient.GetAsync(null, url);

            if (response.IsSuccessStatusCode)
            {
                ChatRoomViewModel.Room = await HTTPClient.ReadAsAsync<RoomInfo>(response);
                return true;
            }
            else return false;
        }

        public enum NotificationSource
        {
            OnLaunch,
            OnResume,
            OnBackground,
            OnForeground,
        }

        public static void OnNotificationReceived(PushNotificationDTO notification, NotificationSource source)
        {
            if (source == NotificationSource.OnLaunch)
                Notification = notification;
            else
                Device.BeginInvokeOnMainThread(() => ProcessNotificationReceived(notification, source));
        }

        private static async void ProcessNotificationReceived(PushNotificationDTO notification, NotificationSource source)
        {
            if (source == NotificationSource.OnResume)
            {
                if (await SetChatRoomPage(notification.UserChatRoomId))
                {
                    await Shell.Current.GoToAsync(nameof(ChatRoomPage));
                }
            }
            else
            {
                if (notification.Topic == PushNotificationTopic.MessageSent)
                {
                    string args = $"?messageSentId={notification.MessageSentId}&dateReceived={DateTimeOffset.Now}";
                    var response = await HTTPClient.PostAsync<string>(null, "/api/Message/Received" + args, null);
                    if (response.IsSuccessStatusCode)
                    {
                        var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);

                        if (source == NotificationSource.OnForeground &&
                            ChatRoomViewModel.Appearing != null &&
                            ChatRoomViewModel.Appearing.AddMessage(message))
                        {
                            try
                            {
                                // see https://docs.microsoft.com/en-us/xamarin/essentials/vibrate
                                Xamarin.Essentials.Vibration.Vibrate(); // Use default vibration length
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            notification.UserChatRoomId = message.ReceiverId; // a hack!
                            DependencyService.Get<INotifications>().Notify(notification);
                        }
                    }
                }
            }
        }
    }
}
