using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Global.Models;
using XamApp.Services;
using Xamarin.Forms;
using Global.Enums;

namespace XamApp.ViewModels
{
    public class ChatRoomViewModel : BaseViewModel
    {
        public static ChatRoomViewModel Appearing = null;
        public static RoomInfo Room;
        private RoomInfo _room;
        private string _body;

        public Command SendMessageCommand { get; }
        public ObservableCollection<Message> Messages { get; }

        public ChatRoomViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            Messages = new ObservableCollection<Message>();
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            OnPropertyChanged(nameof(ShowSendButton));
        }

        public bool AddMessage(MessageSentDTO message)
        {
            if (_room != null && message.MessageTag.ChatRoomId == _room.Id)
            {
                Messages.Add(new Message(_room, message));
                ScrollTo(-1);
                return true;
            }
            else return false;
        }

        public void OnDisappearing()
        {
            Appearing = null;
        }

        public Task OnAppearing()
        {
            if (Room != null)
            {
                _room = Room;
                Room = null;
                Title = _room.Name;
            }
            Appearing = this;
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
                    ScrollTo(-1);
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
        private void ScrollTo(int index)
        {
            if (index < 0)
                index += Messages.Count;
            if (index >= 0)
                MessagesView.ScrollTo(index);
        }

        public async Task SendMessage(FileDTO file)
        {
            SetBusy(true);
            var messageSent = new MessageSentDTO()
            {
                SenderId = _room.UserChatRoomId,
                MessageTag = new MessageTagDTO { ChatRoomId = _room.Id },
                Body = Body,
                FileId = file?.Id,
                MessageType = file == null ? MessageTypeEnum.Text : MessageTypeEnum.File,
                DateSent = DateTimeOffset.Now,
            };
            var response = await HTTPClient.PostAsync(null, "/api/Message", messageSent);
            if (response.IsSuccessStatusCode)
            {
                var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);
                message.File = file;

                Messages.Add(new Message(_room, message));
                ScrollTo(-1);
            }
            else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
            Body = null;
            SetBusy(false);
        }

        private async void SendMessage()
        {
            if (CanSendMessage)
            {
                await SendMessage(null);
            }
        }
    }
}
