using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
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
                var response = await HTTPClient.GetAsync(null, "/api/ChatRoom/GetAll");
                if (response.IsSuccessStatusCode)
                {
                    var rooms = await HTTPClient.ReadAsAsync<List<RoomInfo>>(response);
                    Rooms.Clear();
                    foreach (var room in rooms)
                        Rooms.Add(room);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                IsBusy = false;
            }
        }

        private async void AddChatRoom(object obj)
        {
            await DisplayAlert("Not Available", "The feature to add a chat room is not yet implemented!", "Ok");
        }

        private async void OnRoomSelected(RoomInfo room)
        {
            if (room != null && !IsBusy)
            {
                ChatRoomViewModel.Room = room; // provide necessary data
                // This will push the ChatRoomPage onto the navigation stack
                await Shell.Current.GoToAsync(nameof(Views.ChatRoomPage));
            }
        }
    }
}
