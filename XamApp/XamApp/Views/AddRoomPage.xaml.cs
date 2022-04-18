using System;
using System.Web;
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
            base.OnAppearing();
            SetBusy(false);
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            vm.IsBusy = busy;
            vm.UpdateButton();
        }

        public const string separator = ": ";

        private async void AddChatRoom(object sender, EventArgs e)
        {
            if (vm.CanAdd)
            {
                SetBusy(true);
                string url;
                GroupProfileDTO model = null;

                if (vm.GroupName != null) // if adding a new group
                {
                    bool confirmed = await DisplayAlert("Confirm", "Please confirm you want to create the group:\n\n" + vm.GroupName, "Ok", "Cancel");
                    if (!confirmed)
                    {
                        SetBusy(false);
                        return;
                    }

                    model = new GroupProfileDTO()
                    {
                        GroupName = vm.GroupName,
                        JoinToken = vm.GroupName,
                    };
                    url = "/api/GroupProfile";
                }
                else if (vm.JoinToken != null) // if joining a group
                {
                    string joinToken = null;
                    long chatRoomId = 0;

                    int index = vm.JoinToken.IndexOf(separator);
                    if (index >= 0)
                    {
                        string id = vm.JoinToken.Substring(0, index);
                        long.TryParse(id, out chatRoomId);

                        index += separator.Length;
                        if (index < vm.JoinToken.Length)
                            joinToken = vm.JoinToken.Substring(index);
                    }

                    url = "/api/GroupProfile/JoinGroup?id=" + chatRoomId;
                    url += "&joinToken=" + HttpUtility.UrlEncode(joinToken);
                }
                else // if creating a private chat room with another user
                {
                    string accountID = HttpUtility.UrlEncode(vm.AccountID);
                    url = "/api/ChatRoom/CreatePrivate?accountID=" + accountID;
                }

                var response = await HTTPClient.PostAsync(null, url, model);
                if (response.IsSuccessStatusCode)
                {
                    var userChatRoom = await HTTPClient.ReadAsAsync<UserChatRoomDTO>(response);

                    if (vm.GroupName != null) // if adding a new group
                    {
                        var groupProfile = userChatRoom.ChatRoom.GroupProfile;
                        string joinToken = groupProfile.ChatRoomId + separator + groupProfile.JoinToken;

                        await Clipboard.SetTextAsync(joinToken);
                        App.Toast("Token copied", true);

                        string message = "Below is the token for others to join your group. It has been copied to the clipboard!\n\n" + joinToken;
                        await DisplayAlert("Group Created", message, "Ok");
                    }

                    await Shell.Current.GoToAsync("../" + nameof(ChatRoomPage) + "?id=" + userChatRoom.ChatRoomId);
                }
                else
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Failed to Add", message, "Ok");
                }
                SetBusy(false);
            }
            else await DisplayAlert("Cannot Proceed", "Please first correct the information provided.", "Ok");
        }
    }
}