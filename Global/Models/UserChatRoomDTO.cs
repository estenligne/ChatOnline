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

        public DateTime DateAdded { get; set; }
        public DateTime? DateBlocked { get; set; }

        public DateTime? DateDeleted { get; set; }
        public DateTime? DateExited { get; set; }

        public DateTime? DateMuted { get; set; }
        public TimeSpan? MuteDuration { get; set; }
    }
}
