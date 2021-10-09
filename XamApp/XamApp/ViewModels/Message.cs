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
        private readonly RoomInfo _room;
        private readonly MessageSentDTO _message;
        private readonly Message _linked;

        public Message(ChatRoomViewModel chatRoomVM, RoomInfo room, MessageSentDTO message)
        {
            _room = room;
            _message = message;
            _linked = chatRoomVM.GetMessageById(message.LinkedId);
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

        public bool IamSender => _message.DateReceived == null;
        public bool AmNotSender => !IamSender;
        public bool IsGroupChat => _room.Type == ChatRoomTypeEnum.Group;

        public long Id => _message.Id;
        public string Sender => _message.SenderName;

        public string Body => _message.Body;
        public string ShortBody => BasicHelpers.GetShortBody(Body, 100);
        public bool HasBody => !string.IsNullOrEmpty(Body);

        public string LinkedMessageSender => _linked?.Sender;
        public string LinkedMessageBody => _linked?.ShortBody;
        public bool HasLinkedMessage => _linked != null;

    }
}
