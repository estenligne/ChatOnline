using System;
using Global.Enums;

namespace Global.Models
{
    public class ChatRoomDTO
    {
        public long Id { get; set; }
        public ChatRoomTypeEnum Type { get; set; }

        public long? GroupProfileId { get; set; }
        public GroupProfileDTO GroupProfile { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
