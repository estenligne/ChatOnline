﻿using System;
using Global.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
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

        private const string separator = ": ";

        private async void AddChatRoom(object sender, EventArgs e)
        {
            if (vm.CanAdd)
            {
                SetBusy(true);
                string url;
                GroupProfileDTO model = null;
                User user = await DataStore.Instance.GetUserAsync();

                if (vm.GroupName != null) // if adding a new group
                {
                    model = new GroupProfileDTO()
                    {
                        CreatorId = user.UserProfileId,
                        GroupName = vm.GroupName,
                        JoinToken = vm.GroupName,
                    };
                    url = "/api/GroupProfile";
                }
                else if (vm.JoinToken != null) // if joining a group
                {
                    string joinToken = null;
                    long groupProfileId = 0;

                    int index = vm.JoinToken.IndexOf(separator);
                    if (index >= 0)
                    {
                        string id = vm.JoinToken.Substring(0, index);
                        long.TryParse(id, out groupProfileId);

                        index += separator.Length;
                        if (index < vm.JoinToken.Length)
                            joinToken = vm.JoinToken.Substring(index);
                    }

                    url = "/api/GroupProfile/JoinGroup";
                    url += "?userProfileId=" + user.UserProfileId;
                    url += "&groupProfileId=" + groupProfileId;
                    url += "&joinToken=" + joinToken;
                }
                else // if creating a private chat room with another user
                {
                    url = "/api/ChatRoom/CreatePrivate";
                    url += "?userProfileId=" + user.UserProfileId;
                    url += "&emailAddress=" + vm.Email;
                    url += "&phoneNumber=" + vm.PhoneNumber;
                }

                var response = await HTTPClient.PostAsync(null, url, model);
                if (response.IsSuccessStatusCode)
                {
                    var userChatRoom = await HTTPClient.ReadAsAsync<UserChatRoomDTO>(response);

                    if (vm.GroupName != null) // if adding a new group
                    {
                        var groupProfile = userChatRoom.ChatRoom.GroupProfile;
                        string joinToken = groupProfile.Id + separator + groupProfile.JoinToken;
                        await Clipboard.SetTextAsync(joinToken);

                        string message = "Below is the token for others to join your new group. It has already been copied to the clipboard!\n\n" + joinToken;
                        await DisplayAlert("New Group Created", message, "Ok");
                    }

                    url = "/api/ChatRoom/GetInfo?userChatRoomId=" + userChatRoom.Id;
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