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
    public partial class UserProfilePage : ContentPage
    {
        private readonly UserProfileViewModel vm;

        public UserProfilePage()
        {
            InitializeComponent();
            vm = new UserProfileViewModel();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            vm.IsBusy = busy;
            vm.UpdateButton();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (vm.CanSave)
            {
                SetBusy(true);

                User user = await DataStore.Instance.GetUserAsync();

                var userProfile = new UserProfileDTO()
                {
                    Identity = user.Identity,
                    Username = vm.Name,
                    About = vm.About,
                };

                if (vm.userProfileId == 0) // if creating
                {
                    var response = await HTTPClient.PostAsync(null, "/api/UserProfile", userProfile);
                    if (response.IsSuccessStatusCode)
                    {
                        await GotoRoomsPage(user);
                    }
                    else await DisplayAlert("Failed to Create", await HTTPClient.GetResponseError(response), "Ok");
                }
                else // if updating
                {
                    var response = await HTTPClient.PutAsync(null, "/api/UserProfile", userProfile);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Saved!", "Ok");
                    }
                    else await DisplayAlert("Failed to Save", await HTTPClient.GetResponseError(response), "Ok");
                }

                SetBusy(false);
            }
        }

        private async Task GotoRoomsPage(User user)
        {
            var url = "/api/DeviceUsed/GetOrCreate?devicePlatform=" + DevicePlatformEnum.Unknown;
            var response = await HTTPClient.GetAsync(null, url);

            if (response.IsSuccessStatusCode)
            {
                var deviceUsedDto = await HTTPClient.ReadAsAsync<DeviceUsedDTO>(response);

                user.DeviceUsedId = deviceUsedDto.Id;
                user.UserProfileId = deviceUsedDto.UserProfileId;
                await DataStore.Instance.UpdateUserAsync(user);

                DependencyService.Get<INotifications>().RegisterFcmToken(deviceUsedDto.Id);

                // Prefixing with `//` switches to a different
                // navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
            }
            else await DisplayAlert("Failed", await HTTPClient.GetResponseError(response), "Ok");
        }
    }
}