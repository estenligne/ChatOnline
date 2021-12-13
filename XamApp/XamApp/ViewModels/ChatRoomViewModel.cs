using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Global.Enums;
using Global.Models;
using XamApp.Services;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class ChatRoomViewModel : BaseViewModel, IProcessNotification
    {
        public Command SendMessageCommand { get; }
        public ObservableCollection<Message> Messages { get; }
        private Dictionary<long, int> MessageIndex { get; }

        public ChatRoomViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            Messages = new ObservableCollection<Message>();
            MessageIndex = new Dictionary<long, int>();
        }

        private void SetBusy(bool busy)
        {
            IsBusy = busy;
            OnPropertyChanged(nameof(ShowSendButton));
        }

        private int GetNextIndex(Message message)
        {
            int count = Messages.Count;
            Message last = count != 0 ? Messages[count - 1] : null;

            if (last == null || (last.DateSent.Date != message.DateSent.Date))
            {
                var msg = new MessageSentDTO
                {
                    MessageType = MessageTypeEnum.SwitchInDate,
                    DateSent = message.DateSent,
                };
                Messages.Add(new Message(this, msg));
                count += 1;
            }
            return count;
        }

        private void AddMessage(MessageSentDTO message)
        {
            Message msg = new Message(this, message);
            MessageIndex[msg.Id] = GetNextIndex(msg);
            Messages.Add(msg);
        }

        bool IProcessNotification.ProcessPushNotification(PushNotificationDTO notification)
        {
            if (notification.MessageSent.MessageTag.ChatRoomId == Room.Id)
            {
                if (notification.Topic == PushNotificationTopic.MessageSent)
                {
                    AddMessage(notification.MessageSent);
                    ScrollToIndex(-1);
                    return true;
                }
            }
            return false;
        }

        private RoomInfo _room;
        public RoomInfo Room
        {
            get { return _room; }
            set
            {
                _room = value;
                Title = _room.Name;
            }
        }

        public void OnDisappearing()
        {
            App.Appearing = null;
        }

        public Task OnAppearing()
        {
            App.Appearing = this;
            return LoadMessages();
        }

        private async Task LoadMessages()
        {
            if (!IsBusy)
            {
                SetBusy(true);
                var url = "/api/Message/GetMany?userChatRoomId=" + Room.UserChatRoomId;
                var response = await HTTPClient.GetAsync(null, url);
                if (response.IsSuccessStatusCode)
                {
                    var messages = await HTTPClient.ReadAsAsync<List<MessageSentDTO>>(response);

                    Messages.Clear();
                    MessageIndex.Clear();

                    foreach (var message in messages)
                        AddMessage(message);

                    ScrollToIndex(-1);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                SetBusy(false);
            }
        }

        public void UpdateMessageView(Message message)
        {
            int index = GetMessageIndex(message.Id);
            Messages[index] = message; // trigger update
        }

        public int GetMessageIndex(long? id)
        {
            int index = -1;
            if (id != null && !MessageIndex.TryGetValue(id.Value, out index))
            {
                Trace.TraceError($"Message {id} not found");
            }
            return index;
        }

        public Message GetMessage(long? id, int offset = 0)
        {
            int index = GetMessageIndex(id);
            if (index >= 0)
            {
                index += offset;
                if (0 <= index && index < Messages.Count)
                    return Messages[index];
            }
            return null;
        }

        private string _body;
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

        private Message _linked;
        public void SetLinkedMessage(Message linked)
        {
            _linked = linked;
            OnPropertyChanged(nameof(LinkedMessageSender));
            OnPropertyChanged(nameof(LinkedMessageBody));
            OnPropertyChanged(nameof(HasLinkedMessage));
        }

        public string LinkedMessageSender => _linked?.Sender;
        public string LinkedMessageBody => _linked?.ShortBody;
        public bool HasLinkedMessage => _linked != null;

        public bool IsGroupChat => Room.Type == ChatRoomTypeEnum.Group;

        public bool ShowSendButton => !IsBusy;
        public bool CanSendMessage => ShowSendButton && !string.IsNullOrEmpty(Body);

        public CollectionView MessagesView;
        private void ScrollToIndex(int index)
        {
            if (index < 0)
                index += Messages.Count;
            if (index >= 0)
                MessagesView.ScrollTo(index);
        }
        public void ScrollToMessage(long id)
        {
            MessagesView.ScrollTo(GetMessageIndex(id), position: ScrollToPosition.Center);
        }

        public async Task SendMessage(FileDTO file)
        {
            SetBusy(true);

            var messageSent = new MessageSentDTO()
            {
                SenderId = Room.UserChatRoomId,
                MessageTag = new MessageTagDTO { ChatRoomId = Room.Id },
                Body = Body,
                FileId = file?.Id,
                MessageType = file == null ? MessageTypeEnum.Text : MessageTypeEnum.File,
                LinkedId = _linked?.Id,
                DateSent = DateTimeOffset.Now,
            };

            var response = await HTTPClient.PostAsync(null, "/api/Message", messageSent);
            if (response.IsSuccessStatusCode)
            {
                var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);
                message.File = file;

                AddMessage(message);
                ScrollToIndex(-1);
            }
            else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");

            Body = null;
            SetLinkedMessage(null);
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
