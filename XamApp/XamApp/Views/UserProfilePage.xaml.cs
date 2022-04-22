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
using Xamarin.Essentials;
using System.IO;

namespace XamApp.Views
{
    [QueryProperty(nameof(UserId), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserProfilePage : ContentPage
    {
        private readonly UserProfileViewModel vm;
        public long UserId { get; set; }

        public UserProfilePage()
        {
            InitializeComponent();
            vm = new UserProfileViewModel();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            vm.user.Id = UserId;
            await vm.OnAppearing();

            if (!vm.IsCurrentUser || vm.user.Id == 0)
            {
                ToolbarItems.Clear();
            }
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
            if (!vm.CanEdit)
            {
                vm.CanEdit = true;
            }
            else if (vm.CanSave)
            {
                SetBusy(true);

                if(vm.ImageChosen != null)
                {
                    vm.ImageChosen.Position = 0;

                    var response = await HTTPClient.PostFile(null, "file", "profile photo", vm.ImageChosen);
                    if (response.IsSuccessStatusCode)
                    {
                        FileDTO fileDTO = await HTTPClient.ReadAsAsync<FileDTO>(response);
                        vm.user.PhotoFileId = fileDTO.Id;
                        vm.user.PhotoFile = fileDTO;
                    }
                    else
                    {
                        await DisplayAlert("ERROR", await HTTPClient.GetResponseError(response), "OK");
                        SetBusy(false);
                        return;
                    }
                }

                if (vm.user.Id == 0) // if creating
                {
                    var response = await HTTPClient.PostAsync(null, "/api/UserProfile", vm.user);
                    if (response.IsSuccessStatusCode)
                    {
                        await GotoRoomsPage();
                    }
                    else await DisplayAlert("Failed to Create", await HTTPClient.GetResponseError(response), "Ok");
                }
                else // if updating
                {
                    var response = await HTTPClient.PutAsync(null, "/api/UserProfile", vm.user);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Saved!", "Ok");
                    }
                    else await DisplayAlert("Failed to Save", await HTTPClient.GetResponseError(response), "Ok");
                }

                SetBusy(false);
            }
        }

        private async Task GotoRoomsPage()
        {
            string url = SignInPage.GetURLtoPutDeviceUsed();
            var response = await HTTPClient.PutAsync<string>(null, url, null);

            if (response.IsSuccessStatusCode)
            {
                var deviceUsedDto = await HTTPClient.ReadAsAsync<DeviceUsedDTO>(response);

                User user = await DataStore.Instance.GetUserAsync();
                user.DeviceUsedId = deviceUsedDto.Id;
                await DataStore.Instance.UpdateUserAsync(user);

                DependencyService.Get<INotifications>().RegisterFcmToken(deviceUsedDto.Id);

                // Prefixing with `//` switches to a different
                // navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
            }
            else await DisplayAlert("Failed", await HTTPClient.GetResponseError(response), "Ok");
        }

        private void OnEditOrOnViewButtonCliked(object sender, EventArgs e)
        {
            if (vm.OnbuttonClicked == "Edit")
                vm.CanEdit = true;
            else
                vm.CanEdit = false;
        }

        private async void OnUserPhotoclicked(object sender, EventArgs e)
        {
            if (vm.CanEdit)
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a photo"
                });

                vm.ImageChosen = new MemoryStream();
                using (var stream = await result.OpenReadAsync())
                    stream.CopyTo(vm.ImageChosen);
            }
        }
    }
}