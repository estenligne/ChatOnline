using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.MessageTags))]
    public class MessageTag
    {
        public long Id { get; set; }

        [MaxLength(63)]
        public string Name { get; set; }

        public long ChatRoomId { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }

        public long CreatorId { get; set; }
        public virtual UserProfile Creator { get; set; }

        public DateTime DateCreated { get; set; }
        public bool IsPrivate { get; set; }
    }
}
