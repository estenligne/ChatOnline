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
        }

        private async void OnSignOutClicked(object sender, EventArgs e)
        {
            User user = IsBusy ? null : await DataStore.Instance.GetUserAsync();
            if (user != null)
            {
                IsBusy = true;
                var url = "/api/Account/SignOut?deviceUsedId=" + user.DeviceUsedId;

                var response = await HTTPClient.PostAsync<string>(null, url, null);
                if (!response.IsSuccessStatusCode)
                {
                    string message = await HTTPClient.GetResponseError(response);
                    message += "\n\nFor your security, you must please clear all app data if this persists.";
                    await DisplayAlert("Sign Out Failed", message, "Ok");
                }
                else
                {
                    HTTPClient.Clear();
                    await DataStore.Instance.DeleteUserAsync();
                    await Shell.Current.GoToAsync("//" + nameof(LoginPage));
                }
                IsBusy = false;
            }
        }
    }
}
