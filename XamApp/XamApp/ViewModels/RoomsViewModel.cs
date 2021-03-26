using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using XamApp.Services;

namespace XamApp.ViewModels
{
    public class RoomsViewModel : BaseViewModel
    {
        private RoomInfo _selectedRoom;
        public ObservableCollection<RoomInfo> Rooms { get; }

        public Command LoadRoomsCommand { get; }
        public Command AddRoomCommand { get; }
        public Command<RoomInfo> RoomTapped { get; }

        public RoomsViewModel()
        {
            Title = "Chat Rooms";

            Rooms = new ObservableCollection<RoomInfo>();

            LoadRoomsCommand = new Command(async () => await LoadRooms());

            RoomTapped = new Command<RoomInfo>(OnRoomSelected);

            AddRoomCommand = new Command(AddChatRoom);
        }

        private async Task LoadRooms()
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
            else
            {
                var message = await HTTPClient.GetResponseError(response);
                await DisplayAlert("Request Error", message, "Ok");
            }
            IsBusy = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
            SelectedRoom = null;
        }

        public RoomInfo SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                SetProperty(ref _selectedRoom, value);
                OnRoomSelected(value);
            }
        }

        private async void AddChatRoom(object obj)
        {
            await DisplayAlert("Not Available", "The feature to add a chat room is not yet implemented!", "Ok");
        }

        private async void OnRoomSelected(RoomInfo item)
        {
            if (item != null)
            {
                await DisplayAlert("Not Available", "The feature to view a chat room is not yet implemented!", "Ok");
            }
        }
    }
}
