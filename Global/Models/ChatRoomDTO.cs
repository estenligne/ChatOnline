using System;
using Global.Enums;

namespace Global.Models
{
    public class ChatRoomDTO
    {
        public long Id { get; set; }
        public ChatRoomTypeEnum Type { get; set; }

        public long CreatorId { get; set; }
        public UserProfileDTO Creator { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        public GroupProfileDTO GroupProfile { get; set; }
    }
}
