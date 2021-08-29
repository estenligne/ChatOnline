using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.MessagesReceived))]
    [Index(nameof(ReceiverId), nameof(MessageSentId), IsUnique = true)]
    public class MessageReceived
    {
        public long Id { get; set; }

        public long ReceiverId { get; set; }
        public virtual UserChatRoom Receiver { get; set; }

        public long MessageSentId { get; set; }
        public virtual MessageSent MessageSent { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateReceived { get; set; }
        public DateTimeOffset? DateRead { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
        public DateTimeOffset? DateStarred { get; set; }

        public MessageReactionEnum Reaction { get; set; }
    }
}
