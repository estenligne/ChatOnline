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
    public partial class SignInPage : ContentPage
    {
        private readonly SignInViewModel vm;

        public SignInPage()
        {
            InitializeComponent();
            vm = new SignInViewModel();
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
                    PhoneNumber = vm.PhoneNumber,
                    Password = vm.Password,
                    RememberMe = true,
                };

                var response = await HTTPClient.PostAsync(null, "/api/Account/SignIn", userDto);
                if (response.IsSuccessStatusCode)
                {
                    var result = await HTTPClient.ReadAsAsync<ApplicationUserDTO>(response);

                    var user = new User()
                    {
                        Id = result.Id,
                        Email = userDto.Email,
                        PhoneNumber = userDto.PhoneNumber,
                        Password = userDto.Password,
                        RememberMe = userDto.RememberMe,
                        Authorization = result.Authorization,
                    };
                    HTTPClient.SetAuthorization(null, user.Authorization);

                    var url = "/api/DeviceUsed?devicePlatform=" + DevicePlatformEnum.Unknown;
                    response = await HTTPClient.PutAsync<string>(null, url, null);

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
                        await DataStore.Instance.InsertUserAsync(user);
                        await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}/{nameof(UserProfilePage)}");
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
                var response = await HTTPClient.PatchAsync(null, "/api/Account/ForgotPassword" + args, (string)null);
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
            if (!vm.IsBusy)
                vm.UpdateChoice(true);
        }

        private void GotoRegister(object sender, EventArgs e)
        {
            if (!vm.IsBusy)
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
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        await DisplayAlert("Success", "Now, go sign in!", "Ok");
                        vm.UpdateChoice(true);
                    }
                    else if (string.IsNullOrEmpty(vm.PhoneNumber))
                    {
                        string message = $"A registration confirmation email has been sent to {vm.Email}.";
                        message += "\n\nPlease click on the link provided to confirm your account.";
                        message += "\n\nPlease also check your Spam or Junk folder for the email.";
                        await DisplayAlert("Confirm Your Account", message, "Ok");
                    }
                    else
                    {
                        string message = $"A registration confirmation SMS has been sent to {vm.PhoneNumber}.";
                        message += "\n\nPlease click on the link provided to confirm your account.";
                        await DisplayAlert("Confirm Your Account", message, "Ok");
                    }
                    vm.UpdateChoice(true);
                }
                else await DisplayAlert("Failed to Register", await HTTPClient.GetResponseError(response), "Ok");

                SetBusy(false);
            }
        }
    }
}
