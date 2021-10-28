using System;
using System.IO;
using Global.Enums;
using Global.Helpers;
using Global.Models;
using XamApp.Services;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class Message
    {
        private readonly ChatRoomViewModel _chatRoom;
        private readonly MessageSentDTO _message;
        private readonly Message _linked;
        private readonly EventDTO _eventDto;

        public Message(ChatRoomViewModel chatRoom, MessageSentDTO message)
        {
            _chatRoom = chatRoom;
            _message = message;
            _linked = chatRoom.GetMessage(message.LinkedId);
        }

        public Message(EventDTO eventDto)
        {
            _eventDto = eventDto;
        }

        public bool IsAnEvent => _eventDto != null || _message.DateDeleted != null;
        public DateTimeOffset DateOccurred => _eventDto != null ? _eventDto.DateOccurred : _message.DateSent;

        private ChatRoomEventEnum Event => _eventDto != null ? _eventDto.Event :
            _message?.DateDeleted != null ? ChatRoomEventEnum.MessageDeleted :
            ChatRoomEventEnum.None;

        public Color EventBackgroundColor
        {
            get
            {
                switch (Event)
                {
                    case ChatRoomEventEnum.DateChanged:
                        return Color.DarkCyan;

                    case ChatRoomEventEnum.MessageDeleted:
                        return Color.Gray;

                    default: return Color.Black;
                }
            }
        }

        public string EventMessage
        {
            get
            {
                switch (Event)
                {
                    case ChatRoomEventEnum.DateChanged:
                        return DateOccurred.ToLocalTime().ToString("dd/MM/yyyy");

                    case ChatRoomEventEnum.MessageDeleted:
                        return "Message deleted at " + _message.DateDeleted.Value.ToString("HH:mm");

                    default: return null;
                }
            }
        }

        private ImageSource imageFile;
        public ImageSource ImageFile
        {
            get
            {
                if (imageFile == null && HasImage) // if not already loaded
                {
                    string path = "/api/File/Download?fileName=" + _message.File.Name;
                    imageFile = ImageSource.FromUri(new Uri(HTTPClient.WebAPIBaseURL + path));
                }
                return imageFile;
            }
        }

        public bool HasImage
        {
            get
            {
                if (_message.File == null)
                    return false;
                try
                {
                    string ext = Path.GetExtension(_message.File.Name).ToLower();
                    return ext == ".png" || ext == ".jpg" || ext == ".jpeg";
                }
                catch (Exception) { return false; }
            }
        }

        public Color BackgroundColor => IamSender ? Color.LightGreen : Color.LightPink;
        public LayoutOptions HorizontalOptions => IamSender ? LayoutOptions.Start : LayoutOptions.End;

        private bool IamSender => _message.DateReceived == null;
        public bool IsGroupChat => _chatRoom.IsGroupChat;

        public long Id => _message.Id;
        public string Sender => _message.SenderName;

        public string Body => _message.Body;
        public string ShortBody => BasicHelpers.GetShortBody(Body, 100);
        public bool HasBody => !string.IsNullOrEmpty(Body);

        public string FooterInfo
        {
            get
            {
                string timeSent = _message.DateSent.ToLocalTime().ToString("HH:mm");
                return timeSent;
            }
        }

        public string LinkedMessageSender => _linked?.Sender;
        public string LinkedMessageBody => _linked?.ShortBody;
        public bool HasLinkedMessage => _linked != null;

        public void Delete(DateTimeOffset dateDeleted)
        {
            _message.DateDeleted = dateDeleted;
            int index = _chatRoom.GetMessageIndex(Id);
            _chatRoom.Messages[index] = this; // update
        }
    }
}
