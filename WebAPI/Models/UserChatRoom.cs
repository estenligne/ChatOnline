using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.UserChatRooms))]
    public class UserChatRoom
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        public long ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }

        public UserRoleEnum UserRole { get; set; }

        public long? AdderId { get; set; }
        public virtual UserProfile Adder { get; set; }

        public long? BlockerId { get; set; }
        public virtual UserProfile Blocker { get; set; }

        public DateTime DateAdded { get; set; }
        public DateTime? DateBlocked { get; set; }

        public DateTime? DateDeleted { get; set; }
        public DateTime? DateExited { get; set; }

        public DateTime? DateMuted { get; set; }
        public DateTime? DatePinned { get; set; }
    }
}
