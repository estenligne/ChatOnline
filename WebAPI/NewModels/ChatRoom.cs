using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.NewModels
{
    [Index(nameof(CreatorId), nameof(Type))]
    public class ChatRoom
    {
        public long Id { get; set; }
        public ChatRoomTypeEnum Type { get; set; }

        public long CreatorId { get; set; }
        public virtual UserProfile Creator { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        public virtual GroupProfile GroupProfile { get; set; }
    }
}
