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
            _linked = chatRoom.GetMessageById(message.LinkedId);
        }

        public Message(EventDTO eventDto)
        {
            _eventDto = eventDto;
        }

        public bool IsAnEvent => _message == null;
        public bool NotAnEvent => !IsAnEvent;
        public DateTimeOffset DateOccurred => IsAnEvent ? _eventDto.DateOccurred : _message.DateSent;

        public string EventMessage
        {
            get
            {
                switch (_eventDto?.Event)
                {
                    case ChatRoomEventEnum.DateChanged:
                        return DateOccurred.ToLocalTime().ToString("dd/MM/yyyy");

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

    }
}
