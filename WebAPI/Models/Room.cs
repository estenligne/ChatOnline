using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    [Index(nameof(CreatorId))]
    public class Room
    {
        public long Id { get; set; }
        public ChatRoomTypeEnum Type { get; set; }

        public long CreatorId { get; set; }
        public virtual User Creator { get; set; }

        public long DateCreated { get; set; }
        public long? DateDeleted { get; set; }

        public virtual Group GroupProfile { get; set; }
    }
}
