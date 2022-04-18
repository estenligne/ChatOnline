using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.NewModels
{
    [Index(nameof(UserProfileId), nameof(ChatRoomId), IsUnique = true)]
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

        public DateTimeOffset DateAdded { get; set; }
        public DateTimeOffset? DateBlocked { get; set; }

        public DateTimeOffset? DateDeleted { get; set; }
        public DateTimeOffset? DateExited { get; set; }

        public DateTimeOffset? DateMuted { get; set; }
        public DateTimeOffset? DatePinned { get; set; }
    }
}
