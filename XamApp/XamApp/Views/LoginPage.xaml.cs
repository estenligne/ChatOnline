using System;
using System.Net;
using System.Threading.Tasks;
using Global.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamApp.ViewModels;
using XamApp.Models;
using XamApp.Services;

namespace XamApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel vm;

        public LoginPage()
        {
            InitializeComponent();
            vm = new LoginViewModel();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            vm.Password = null;
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            vm.IsBusy = busy;
            vm.UpdateButtons();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (vm.CanLogin)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO();
                userDto.Email = vm.Email;
                userDto.Password = vm.Password;
                userDto.PhoneNumber = vm.PhoneNumber;
                userDto.RememberMe = true;

                var response = await HTTPClient.PostAsync(null, "/api/User/Login", userDto);
                if (response.IsSuccessStatusCode)
                {
                    var user = new User()
                    {
                        Email = userDto.Email,
                        Password = userDto.Password,
                        PhoneNumber = userDto.PhoneNumber,
                    };
                    await DataStore.Instance.InsertUserAsync(user);

                    // Prefixing with `//` switches to a different
                    // navigation stack instead of pushing to the active one
                    await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
                }
                else
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Login Error", message, "Ok");
                }

                SetBusy(false);
            }
        }

        private async void OnChangePasswordClicked(object sender, EventArgs e)
        {
            if (vm.CanChangePassword)
            {
                await DisplayAlert("Not Available", "The feature to change password is not yet available!", "Ok");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (vm.CanRegister)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO();
                userDto.Email = vm.Email;
                userDto.Password = vm.Password;
                userDto.PhoneNumber = vm.PhoneNumber;
                userDto.ProfileName = vm.ProfileName;

                var response = await HTTPClient.PostAsync(null, "/api/User/Register", userDto);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Confirm Your Account", $"A registration confirmation email has been sent to {vm.Email}. Please click on the link provided to confirm your account.", "Ok");
                }
                else
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Failed to Register", message, "Ok");
                }
                SetBusy(false);
            }
        }
    }
}
