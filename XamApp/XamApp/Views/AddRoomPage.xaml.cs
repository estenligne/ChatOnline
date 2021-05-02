using System;
using System.Net;
using System.Net.Http;
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
    public partial class AddRoomPage : ContentPage
    {
        private readonly AddRoomViewModel vm;

        public AddRoomPage()
        {
            InitializeComponent();
            vm = new AddRoomViewModel();
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            vm.IsBusy = busy;
            vm.UpdateButton();
        }

        private async void AddChatRoom(object sender, EventArgs e)
        {
            if (vm.CanAdd)
            {
                SetBusy(true);
                HttpResponseMessage response;
                User user = await DataStore.Instance.GetUserAsync();

                if (vm.GroupName != null)
                {
                    var model = new GroupProfileDTO()
                    {
                        CreatorId = user.UserProfileId,
                        GroupName = vm.GroupName,
                        JoinToken = vm.GroupName,
                    };
                    response = await HTTPClient.PostAsync(null, "/api/GroupProfile", model);
                }
                else
                {
                    string url = "/api/ChatRoom/CreatePrivate";
                    url += "?userProfileId=" + user.UserProfileId;
                    url += "&emailAddress=" + vm.Email;
                    url += "&phoneNumber=" + vm.PhoneNumber;
                    response = await HTTPClient.PostAsync<string>(null, url, null);
                }

                if (response.IsSuccessStatusCode)
                {
                    var userChatRoom = await HTTPClient.ReadAsAsync<UserChatRoomDTO>(response);
                    string url = "/api/ChatRoom/GetInfo?userChatRoomId=" + userChatRoom.Id;
                    response = await HTTPClient.GetAsync(null, url);

                    if (response.IsSuccessStatusCode)
                    {
                        var room = await HTTPClient.ReadAsAsync<RoomInfo>(response);
                        ChatRoomViewModel.Room = room; // provide the necessary data
                        // This will push the ChatRoomPage onto the navigation stack
                        await Shell.Current.GoToAsync("../" + nameof(ChatRoomPage));
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Failed to Add", message, "Ok");
                }
                SetBusy(false);
            }
        }
    }
}