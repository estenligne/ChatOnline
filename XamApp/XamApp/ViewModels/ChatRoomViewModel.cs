using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Global.Models;
using XamApp.Services;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class ChatRoomViewModel : BaseViewModel
    {
        public static RoomInfo Room;
        private RoomInfo _room;
        private string _body;

        public Command SendMessageCommand => new Command(async () => await SendMessage());

        public Command LoadMessagesCommand => new Command(async () => await LoadMessages());

        public ObservableCollection<Message> Messages { get; }


        public ChatRoomViewModel()
        {
            Messages = new ObservableCollection<Message>();
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            OnPropertyChanged(nameof(ShowSendButton));
        }

        public Task OnAppearing()
        {
            if (Room != null)
            {
                _room = Room;
                Room = null;
                Title = _room.Name;
            }
            return LoadMessages();
        }

        private async Task LoadMessages()
        {
            if (!IsBusy)
            {
                SetBusy(true);
                var url = "/api/Message/GetMany?userChatRoomId=" + _room.UserChatRoomId;
                var response = await HTTPClient.GetAsync(null, url);
                if (response.IsSuccessStatusCode)
                {
                    var messages = await HTTPClient.ReadAsAsync<List<MessageSentDTO>>(response);
                    Messages.Clear();
                    foreach (var message in messages)
                        Messages.Add(new Message(_room, message));
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                SetBusy(false);
            }
        }

        public string Body
        {
            get { return _body; }
            set
            {
                if (SetProperty(ref _body, value))
                {
                    OnPropertyChanged(nameof(CanSendMessage));
                }
            }
        }

        public bool ShowSendButton => !IsBusy;

        public bool CanSendMessage => ShowSendButton && !string.IsNullOrEmpty(Body);

        public CollectionView MessagesView;
        public void ScrollTo(int index)
        {
            if (index < 0)
                index += Messages.Count;
            if (index >= 0)
                MessagesView.ScrollTo(index);
        }

        private async Task SendMessage()
        {
            if (CanSendMessage)
            {
                SetBusy(true);
                var messageSent = new MessageSentDTO()
                {
                    SenderId = _room.UserChatRoomId,
                    MessageTag = new MessageTagDTO { ChatRoomId = _room.Id },
                    Body = Body,
                    DateSent = DateTime.UtcNow,
                };
                var response = await HTTPClient.PostAsync(null, "/api/Message", messageSent);
                if (response.IsSuccessStatusCode)
                {
                    var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);
                    Messages.Add(new Message(_room, message));
                    ScrollTo(-1);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                Body = null;
                SetBusy(false);
            }
        }
    }
}
