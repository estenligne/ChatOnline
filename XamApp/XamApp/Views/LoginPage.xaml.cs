using System;
using System.Net;
using System.Threading.Tasks;
using Global.Enums;
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
            vm.PhoneNumber = null;
            vm.ProfileName = null;
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
            if (vm.DoNone)
            {
                vm.UpdateChoice(LoginViewModel.Choice.Login);
            }
            else if (vm.CanLogin)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO()
                {
                    Email = vm.Email,
                    Password = vm.Password,
                    PhoneNumber = vm.PhoneNumber,
                    RememberMe = true,
                };

                var response = await HTTPClient.PostAsync(null, "/api/Account/Login", userDto);
                if (response.IsSuccessStatusCode)
                {
                    userDto = await HTTPClient.ReadAsAsync<ApplicationUserDTO>(response);

                    var url = "/api/DeviceUsed/GetOrCreate?devicePlatform=" + DevicePlatformEnum.Unknown;
                    response = await HTTPClient.PostAsync<string>(null, url, null);

                    if (response.IsSuccessStatusCode)
                    {
                        var deviceUsedDto = await HTTPClient.ReadAsAsync<DeviceUsedDTO>(response);

                        var user = new User()
                        {
                            UserId = userDto.Id,
                            Email = userDto.Email,
                            Password = vm.Password,
                            PhoneNumber = userDto.PhoneNumber,
                            DeviceUsedId = deviceUsedDto.Id,
                            UserProfileId = deviceUsedDto.UserProfileId,
                        };
                        await DataStore.Instance.InsertUserAsync(user);

                        DependencyService.Get<INotifications>().RegisterFcmToken(deviceUsedDto.Id);

                        // Prefixing with `//` switches to a different
                        // navigation stack instead of pushing to the active one
                        await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Login Error", message, "Ok");
                }
                SetBusy(false);
            }
        }

        private async void OnResetPasswordClicked(object sender, EventArgs e)
        {
            if (vm.DoNone)
            {
                vm.UpdateChoice(LoginViewModel.Choice.ResetPassword);
            }
            else if (vm.CanResetPassword)
            {
                await DisplayAlert("Not Available", "The feature to reset password is not yet available!", "Ok");
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (vm.DoNone)
            {
                vm.UpdateChoice(LoginViewModel.Choice.Register);
            }
            else if (vm.CanRegister)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO()
                {
                    Email = vm.Email,
                    Password = vm.Password,
                    PhoneNumber = vm.PhoneNumber,
                    ProfileName = vm.ProfileName,
                };

                var response = await HTTPClient.PostAsync(null, "/api/Account/Register", userDto);
                if (response.IsSuccessStatusCode)
                {
                    string message = $"A registration confirmation email has been sent to {vm.Email}.";
                    message += "\n\nPlease click on the link provided to confirm your account.";
                    message += "\n\nPlease also check your Spam or Junk folder for the email.";
                    await DisplayAlert("Confirm Your Account", message, "Ok");
                    vm.UpdateChoice(LoginViewModel.Choice.Login);
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
