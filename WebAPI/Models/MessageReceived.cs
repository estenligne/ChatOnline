using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.MessagesReceived))]
    public class MessageReceived
    {
        public long Id { get; set; }

        public long ReceiverId { get; set; }
        public virtual UserChatRoom Receiver { get; set; }

        public long MessageSentId { get; set; }
        public virtual MessageSent MessageSent { get; set; }

        public DateTime DateReceived { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateStarred { get; set; }
        public MessageReactionEnum Reaction { get; set; }
    }
}
