using Global.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using XamApp.Models;
using XamApp.Services;
using XamApp.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamApp.Views
{
    [QueryProperty(nameof(UserId), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserProfilePage : ContentPage
    {
        protected readonly UserProfileViewModel vm;
        public long UserId { get; set; }

        public UserProfilePage()
        {
            InitializeComponent();
            vm = new UserProfileViewModel();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            if (UserId != 0)
                vm.user.Id = UserId;

            base.OnAppearing();
            await vm.OnAppearing();

            if (vm.HideToolbarButton)
            {
                ToolbarItems.Clear();
            }
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

                if (vm.ImageChosen != null)
                {
                    var content = new MemoryStream(vm.ImageChosen.Content, false);
                    var response = await HTTPClient.PostFile(null, "file", vm.ImageChosen.Name, content);
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
                    response.Dispose();
                    content.Dispose();
                }

                if (vm.OnCreate) // if creating
                {
                    var response = await HTTPClient.PostAsync(null, "/api/UserProfile", vm.user);
                    if (response.IsSuccessStatusCode)
                    {
                        await GotoRoomsPage();
                    }
                    else
                    {
                        await DisplayAlert("Failed to Create", await HTTPClient.GetResponseError(response), "Ok");
                    }
                    response.Dispose();
                }
                else // if updating
                {
                    var response = await HTTPClient.PutAsync(null, "/api/UserProfile", vm.user);
                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Saved!", "Ok");
                        await vm.OnAppearing(true);
                    }
                    else
                    {
                        await DisplayAlert("Failed to Save", await HTTPClient.GetResponseError(response), "Ok");
                    }
                    response.Dispose();
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

                App.Notifications.RegisterFcmToken(deviceUsedDto.Id);

                // Prefixing with `//` switches to a different
                // navigation stack instead of pushing to the active one
                await Shell.Current.GoToAsync($"//{nameof(RoomsPage)}");
            }
            else await DisplayAlert("Failed", await HTTPClient.GetResponseError(response), "Ok");
        }

        private async void OnEditOrOnViewButtonCliked(object sender, EventArgs e)
        {
            vm.CanEdit = !vm.CanEdit;

            if (vm.ToolbarButtonText == "View" || vm.ToolbarButtonText == "Edit")
                await vm.OnAppearing();
        }

        private async void OnUserPhotoclicked(object sender, EventArgs e)
        {
            if (vm.CanEdit)
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Pick a photo"
                });

                if (result != null)
                {
                    using (var stream = await result.OpenReadAsync())
                    {
                        using (var memStream = new MemoryStream())
                        {
                            stream.CopyTo(memStream);

                            var t = new FileDTO();
                            t.Name = result.FileName;
                            t.Size = memStream.Length;
                            t.Content = memStream.ToArray();
                            vm.ImageChosen = t;
                        }
                    }
                }
            }
        }
    }

    public class CurrentUserPage : UserProfilePage
    {
        protected override async void OnAppearing()
        {
            User currentUser = await DataStore.Instance.GetUserAsync();
            if (currentUser != null)
            {
                vm.user.Id = currentUser.Id;
                vm.IsCurrentUser = true;
            }
            base.OnAppearing();
        }
    }
}