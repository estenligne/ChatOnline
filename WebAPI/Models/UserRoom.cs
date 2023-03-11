using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Index(nameof(UserId), nameof(RoomId), IsUnique = true)]
    [Index(nameof(RoomId), nameof(Role))]
    [Index(nameof(AdderId), nameof(RoomId))]
    [Index(nameof(BlockerId), nameof(RoomId))]
    public class UserRoom
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual User User { get; set; }

        public long RoomId { get; set; }
        public virtual Room Room { get; set; }

        public UserRoleEnum Role { get; set; }

        public long? AdderId { get; set; }
        public virtual User Adder { get; set; }

        public long? BlockerId { get; set; }
        public virtual User Blocker { get; set; }

        public long DateAdded { get; set; }
        public long? DateBlocked { get; set; }

        public long? DateDeleted { get; set; }
        public long? DateExited { get; set; }

        public long? DateMuted { get; set; }
        public long? DatePinned { get; set; }

        public long? DateLastTyping { get; set;}

        public long? DateRoleUpdated { get; set; }
    }
}
