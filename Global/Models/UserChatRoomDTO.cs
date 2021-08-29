using System;
using Global.Enums;

namespace Global.Models
{
    public class UserChatRoomDTO
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public UserProfileDTO UserProfile { get; set; }

        public long ChatRoomId { get; set; }
        public ChatRoomDTO ChatRoom { get; set; }

        public UserRoleEnum UserRole { get; set; }

        public long? AdderId { get; set; }
        public long? BlockerId { get; set; }

        public DateTimeOffset DateAdded { get; set; }
        public DateTimeOffset? DateBlocked { get; set; }

        public DateTimeOffset? DateDeleted { get; set; }
        public DateTimeOffset? DateExited { get; set; }

        public DateTimeOffset? DateMuted { get; set; }
        public DateTimeOffset? DatePinned { get; set; }
    }
}
