using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using XamApp.Models;
using XamApp.Services;

namespace XamApp.ViewModels
{
    public class RoomsViewModel : BaseViewModel
    {
        public Command LoadRoomsCommand { get; }
        public Command AddChatRoomCommand { get; }
        public Command<RoomInfo> RoomSelectedCommand { get; }
        public ObservableCollection<RoomInfo> Rooms { get; }

        public RoomsViewModel()
        {
            Title = "Chat Rooms";
            LoadRoomsCommand = new Command(LoadRooms);
            AddChatRoomCommand = new Command(AddChatRoom);
            RoomSelectedCommand = new Command<RoomInfo>(OnRoomSelected);
            Rooms = new ObservableCollection<RoomInfo>();
        }

        public void OnAppearing()
        {
            LoadRoomsCommand.Execute(null);
        }

        private async void LoadRooms()
        {
            if (!IsBusy)
            {
                IsBusy = true;

                var user = await DataStore.Instance.GetUserAsync();
                var url = "/api/ChatRoom/GetAll?userProfileId=" + user.UserProfileId;

                var response = await HTTPClient.GetAsync(null, url);
                if (response.IsSuccessStatusCode)
                {
                    var rooms = await HTTPClient.ReadAsAsync<List<RoomInfo>>(response);
                    rooms.Sort((a, b) => b.LatestMessage.DateSent.CompareTo(a.LatestMessage.DateSent));

                    Rooms.Clear();
                    foreach (var room in rooms)
                        Rooms.Add(room);
                }
                else
                {
                    var message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Error", message, "Ok");
                }
                IsBusy = false;
            }
        }

        private async void AddChatRoom()
        {
            // This will push the AddRoomPage onto the navigation stack
            await Shell.Current.GoToAsync(nameof(Views.AddRoomPage));
        }

        private async void OnRoomSelected(RoomInfo room)
        {
            if (room != null && !IsBusy)
            {
                ChatRoomViewModel.Room = room; // provide the necessary data
                // This will push the ChatRoomPage onto the navigation stack
                await Shell.Current.GoToAsync(nameof(Views.ChatRoomPage));
            }
        }
    }
}
