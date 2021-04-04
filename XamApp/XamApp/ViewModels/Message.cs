using System;
using Global.Enums;
using Global.Models;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class Message
    {
        private readonly RoomInfo _room;
        private readonly MessageSentDTO _message;

        public Message(RoomInfo room, MessageSentDTO message)
        {
            _room = room;
            _message = message;
        }

        public bool IamSender => _message.DateReceived == null;
        public bool AmNotSender => !IamSender;
        public bool IsGroupChat => _room.Type == ChatRoomTypeEnum.Group;

        public string Sender => _message.SenderId.ToString();
        public string Body => _message.Body;
    }
}
