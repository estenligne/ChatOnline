using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Index(nameof(Status), nameof(ReceiverId))]
    [Index(nameof(ReceiverId), nameof(MessageSentId), IsUnique = true)]
    public class MessageReceived
    {
        public Guid Id { get; set; }

        public long ReceiverId { get; set; }
        public virtual User Receiver { get; set; }

        public Guid MessageSentId { get; set; }
        public virtual MessageSent MessageSent { get; set; }

        public long DateCreated { get; set; }
        public long DateReceived { get; set; }
        public long? DateRead { get; set; }
        public long? DateDeleted { get; set; }
        public long? DateStarred { get; set; }
        public long? DateReacted { get; set; }

        public long? DateDownloaded { get; set; }
        public long? DateFileOpened { get; set; }

        public MessageReactionEnum Reaction { get; set; }
        public MessageStatusEnum Status { get; set; }
    }
}
