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
            if (response.IsSuccessStatusCode)
            {
                HTTPClient.Clear();
                await DataStore.Instance.DeleteUserAsync();
                await Shell.Current.GoToAsync("//" + nameof(LoginPage));
            }
            else
            {
                string message = await HTTPClient.GetResponseAsString(response);
                await DisplayAlert("Logout Error", message,  "Ok");
            }
        }
    }
}
