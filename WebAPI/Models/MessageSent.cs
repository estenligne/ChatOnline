using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.MessagesSent))]
    public class MessageSent
    {
        public long Id { get; set; }

        public long SenderId { get; set; }
        public virtual UserChatRoom Sender { get; set; }

        public long MessageTagId { get; set; }
        public virtual MessageTag MessageTag { get; set; }

        public MessageTypeEnum MessageType { get; set; }

        public long? LinkedId { get; set; }
        public virtual MessageSent Linked { get; set; }

        public long? AuthorId { get; set; }
        public virtual UserProfile Author { get; set; }

        public long? FileId { get; set; }
        public virtual File File { get; set; }

        [MaxLength(16383)]
        public string Body { get; set; }

        public DateTime DateSent { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateStarred { get; set; }
    }
}
