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
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var response = await HTTPClient.PostAsync<string>(null, "/api/User/Logout", null);
            if (!response.IsSuccessStatusCode)
            {
                string message = await HTTPClient.GetResponseError(response);
                message += "\n\nYour login details will still be cleared.";
                await DisplayAlert("Logout Error", message,  "Ok");
            }
            HTTPClient.Clear();
            await DataStore.Instance.DeleteUserAsync();
            await Shell.Current.GoToAsync("//" + nameof(LoginPage));
        }
    }
}
