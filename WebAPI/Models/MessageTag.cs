using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.MessageTags))]
    [Index(nameof(ChatRoomId), nameof(Name), IsUnique = true)]
    public class MessageTag
    {
        public long Id { get; set; }

        [MaxLength(63)]
        public string Name { get; set; }

        public long ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }

        public long CreatorId { get; set; }
        public virtual UserProfile Creator { get; set; }

        public long? ParentId { get; set; }
        public virtual MessageTag Parent { get; set; }

        public DateTimeOffset DateCreated { get; set; }
    }
}
