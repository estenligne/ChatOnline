using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamApp.ViewModels;
using XamApp.Models;
using XamApp.Services;
using Global.Models;

namespace XamApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var vm = (LoginViewModel)BindingContext; // get the view model
            if (vm.CanLogin)
            {
                IsBusy = true;
                vm.IsBusy = true;
                vm.UpdateCanLogin();

                var userDto = new ApplicationUserDTO();
                userDto.Password = vm.Password;
                userDto.RememberMe = true;

                if (vm.EmailOrPhone[0] == '+')
                    userDto.PhoneNumber = vm.EmailOrPhone;
                else userDto.Email = vm.EmailOrPhone;

                var response = await HTTPClient.PostAsync(null, "/api/User/Login", userDto);
                if (response.IsSuccessStatusCode)
                {
                    var user = new User()
                    {
                        Email = userDto.Email,
                        PhoneNumber = userDto.PhoneNumber,
                        Password = userDto.Password,
                    };
                    await DataStore.Instance.InsertUserAsync(user);

                    // Prefixing with `//` switches to a different
                    // navigation stack instead of pushing to the active one
                    await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
                }
                else
                {
                    string message = await HTTPClient.GetResponseAsString(response);
                    await DisplayAlert("Login Error", message, "Ok");
                }

                IsBusy = false;
                vm.IsBusy = false;
                vm.UpdateCanLogin();
            }
        }
    }
}
