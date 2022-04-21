using System;
using System.IO;
using System.Windows.Input;
using Global.Enums;
using Global.Helpers;
using Global.Models;
using XamApp.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class Message
    {
        private readonly ChatRoomViewModel _chatRoom;
        private readonly MessageSentDTO _message;
        private readonly Message _linked;

        public Message(ChatRoomViewModel chatRoom, MessageSentDTO message)
        {
            _chatRoom = chatRoom ?? throw new ArgumentNullException(nameof(chatRoom));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _linked = chatRoom.GetMessage(message.LinkedId);
        }

        public MessageTypeEnum Type => _message.DateDeleted != null ? MessageTypeEnum.Deleted : _message.MessageType;
        public bool IsAnEvent => (Type & MessageTypeEnum.MASK) == MessageTypeEnum.Event;
        public DateTimeOffset DateSent => _message.DateSent.ToLocalTime();

        public Color EventBackgroundColor
        {
            get
            {
                switch (Type)
                {
                    case MessageTypeEnum.SwitchInDate:
                        return Color.DarkCyan;

                    case MessageTypeEnum.Deleted:
                        return Color.Gray;

                    default: return Color.Black;
                }
            }
        }

        public string EventMessage
        {
            get
            {
                switch (Type)
                {
                    case MessageTypeEnum.SwitchInDate:
                        return DateSent.ToString("dd/MM/yyyy");

                    case MessageTypeEnum.Deleted:
                        return Sender + " deleted. " + DateSent.ToString("HH:mm");

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

        public Color BackgroundColor => IamSender ? Color.LightGreen : Color.White;
        public LayoutOptions HorizontalOptions =>
            (IsAnEvent && Type != MessageTypeEnum.Deleted) ? LayoutOptions.Center :
            IamSender ? LayoutOptions.End : LayoutOptions.Start;

        private bool IamSender => _message.DateReceived == null;
        public bool IsGroupChat => _chatRoom.IsGroupChat;

        public long Id => _message.Id;
        public string Sender => _message.SenderName;

        public string Body => _message.Body;
        public FormattedString FormattedBody => ConvertToHtml(Body);
        public string ShortBody => BasicHelpers.GetShortBody(Body, 100);
        public bool HasBody => !string.IsNullOrEmpty(Body);

        public string FooterInfo
        {
            get
            {
                string footerInfo = "";

                if (_message.DateStarred != null)
                    footerInfo += "star ";

                footerInfo += DateSent.ToString("HH:mm");
                return footerInfo;
            }
        }

        public string LinkedMessageSender => _linked?.Sender;
        public string LinkedMessageBody => _linked?.ShortBody;
        public bool HasLinkedMessage => _linked != null;
        public long LinkedId => _linked.Id;

        public void Deleted(DateTimeOffset dateDeleted)
        {
            _message.DateDeleted = dateDeleted;
            _chatRoom.UpdateMessageView(this);
        }

        public void Starred(DateTimeOffset dateStarred)
        {
            _message.DateStarred = dateStarred;
            _chatRoom.UpdateMessageView(this);
        }

        public FormattedString ConvertToHtml(string body)
        {
            var result = new FormattedString();
            Span span = new Span();
            span.Text = body;
            span.TextColor = Color.Blue;
            result.Spans.Add(span);
            return result;

            /*string finalResult = "";
            string newString = "";
            foreach (string sb in body.Split(' '))
            {
                if (sb.Contains("http:") || sb.Contains("https:"))
                    newString = $"<a href=\"{sb}\">{sb}</a>";
                else
                    newString = sb;

                finalResult = finalResult + " " + newString;
            }
            return finalResult;*/
        }
    }
}
