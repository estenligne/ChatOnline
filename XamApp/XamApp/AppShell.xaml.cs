using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamApp.Models;
using XamApp.Views;
using XamApp.Services;

namespace XamApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddRoomPage), typeof(AddRoomPage));
            Routing.RegisterRoute(nameof(ChatRoomPage), typeof(ChatRoomPage));
            Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
        }

        private async void OnSignOutClicked(object sender, EventArgs e)
        {
            User user = IsBusy ? null : await DataStore.Instance.GetUserAsync();
            if (user != null)
            {
                IsBusy = true;
                await DoSignOut(this, user);
                IsBusy = false;
            }
        }

        public static async Task DoSignOut(Page page, User user)
        {
            long deviceUsedId = user == null ? 0 : user.DeviceUsedId;
            string url = "/api/DeviceUsed?id=" + deviceUsedId;

            var response = await HTTPClient.DeleteAsync(null, url);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                string message = await HTTPClient.GetResponseError(response);
                message += "\n\nFor your security, you must please clear all app data if this persists.";
                await page.DisplayAlert("Sign Out Failed", message, "Ok");
            }
            else
            {
                HTTPClient.Clear();
                await DataStore.Instance.DeleteUserAsync();
                await Shell.Current.GoToAsync("//" + nameof(SignInPage));
            }
        }
    }
}
