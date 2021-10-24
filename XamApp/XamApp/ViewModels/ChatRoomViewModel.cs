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
    public class ChatRoomViewModel : BaseViewModel
    {
        private static ChatRoomViewModel Appearing = null;
        public static RoomInfo Room;
        private RoomInfo _room;
        private string _body;

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

            if (last == null || last.DateOccurred.ToUniversalTime().Date != message.DateOccurred.ToUniversalTime().Date)
            {
                var eventDto = new EventDTO
                {
                    Event = ChatRoomEventEnum.DateChanged,
                    DateOccurred = message.DateOccurred,
                    DateCreated = DateTimeOffset.Now
                };
                Messages.Add(new Message(eventDto));
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

        public static bool AddMessageFromPushNotification(MessageSentDTO message)
        {
            var vm = Appearing;
            if (vm != null && vm._room != null && vm._room.Id == message.MessageTag.ChatRoomId)
            {
                vm.AddMessage(message);
                vm.ScrollTo(-1);
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
                    MessageIndex.Clear();

                    foreach (var message in messages)
                        AddMessage(message);

                    ScrollTo(-1);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                SetBusy(false);
            }
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

        public bool IsGroupChat => _room.Type == ChatRoomTypeEnum.Group;

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
                LinkedId = _linked?.Id,
                DateSent = DateTimeOffset.Now,
            };

            var response = await HTTPClient.PostAsync(null, "/api/Message", messageSent);
            if (response.IsSuccessStatusCode)
            {
                var message = await HTTPClient.ReadAsAsync<MessageSentDTO>(response);
                message.File = file;

                AddMessage(message);
                ScrollTo(-1);
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
