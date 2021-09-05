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
            vm.UpdateChoice(true);
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            vm.Password = null;
            vm.PasswordConfirm = null;
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            vm.IsBusy = busy;
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            if (vm.CanSignIn)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO()
                {
                    Email = vm.Email,
                    Password = vm.Password,
                    PhoneNumber = vm.PhoneNumber,
                    RememberMe = true,
                };

                var response = await HTTPClient.PostAsync(null, "/api/Account/SignIn", userDto);
                if (response.IsSuccessStatusCode)
                {
                    userDto = await HTTPClient.ReadAsAsync<ApplicationUserDTO>(response);

                    var user = new User()
                    {
                        UserId = userDto.Id,
                        Email = userDto.Email,
                        PhoneNumber = userDto.PhoneNumber,
                        Password = vm.Password,
                        RememberMe = userDto.RememberMe,
                    };

                    var url = "/api/DeviceUsed/GetOrCreate?devicePlatform=" + DevicePlatformEnum.Unknown;
                    response = await HTTPClient.GetAsync(null, url);

                    if (response.IsSuccessStatusCode)
                    {
                        var deviceUsedDto = await HTTPClient.ReadAsAsync<DeviceUsedDTO>(response);

                        user.DeviceUsedId = deviceUsedDto.Id;
                        user.UserProfileId = deviceUsedDto.UserProfileId;
                        await DataStore.Instance.InsertUserAsync(user);

                        DependencyService.Get<INotifications>().RegisterFcmToken(deviceUsedDto.Id);

                        // Prefixing with `//` switches to a different
                        // navigation stack instead of pushing to the active one
                        await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        await DisplayAlert("Success", "Now, go setup your user profile!", "Ok");
                    }
                    else await DisplayAlert("Sign In Error", await HTTPClient.GetResponseError(response), "Ok");
                }
                else await DisplayAlert("Sign In Failed", await HTTPClient.GetResponseError(response), "Ok");

                SetBusy(false);
            }
        }

        private async void ForgotPassword(object sender, EventArgs e)
        {
            if (vm.ValidIdentity)
            {
                SetBusy(true);

                string args = $"?emailAddress={vm.Email}&phoneNumber={vm.PhoneNumber}";
                var response = await HTTPClient.GetAsync(null, "/api/Account/ForgotPassword" + args);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", await response.Content.ReadAsStringAsync(), "Ok");
                }
                else await DisplayAlert("Failed", await HTTPClient.GetResponseError(response), "Ok");

                SetBusy(false);
            }
            else if (!vm.IsBusy)
            {
                await DisplayAlert("Invalid", "Please provide your email address or phone number.", "Ok");
            }
        }

        private void GotoSignIn(object sender, EventArgs e)
        {
            vm.UpdateChoice(true);
        }

        private void GotoRegister(object sender, EventArgs e)
        {
            vm.UpdateChoice(false);
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (vm.CanRegister)
            {
                SetBusy(true);

                var userDto = new ApplicationUserDTO()
                {
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    Password = vm.Password,
                };

                var response = await HTTPClient.PostAsync(null, "/api/Account/Register", userDto);
                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrEmpty(vm.PhoneNumber))
                    {
                        string message = $"A registration confirmation email has been sent to {vm.Email}.";
                        message += "\n\nPlease click on the link provided to confirm your account.";
                        message += "\n\nPlease also check your Spam or Junk folder for the email.";
                        await DisplayAlert("Confirm Your Account", message, "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Success", "Now, go sign in!", "Ok");
                    }
                    vm.UpdateChoice(true);
                }
                else await DisplayAlert("Failed to Register", await HTTPClient.GetResponseError(response), "Ok");

                SetBusy(false);
            }
        }
    }
}
