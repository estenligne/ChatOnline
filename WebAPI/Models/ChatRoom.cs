using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.ChatRooms))]
    public class ChatRoom
    {
        public long Id { get; set; }
        public ChatRoomTypeEnum Type { get; set; }

        public long? GroupProfileId { get; set; }
        public virtual GroupProfile GroupProfile { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
