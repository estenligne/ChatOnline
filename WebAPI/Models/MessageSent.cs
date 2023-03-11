using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;
using Global.Models;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace WebAPI.Models
{
    [Index(nameof(FileId), IsUnique =true)]
    [Index(nameof(RoomId), nameof(DateUserSent))]
    [Index(nameof(Status), nameof(RoomId))]
    [Index(nameof(SenderId), nameof(RoomId))]
    [Index(nameof(AuthorId), nameof(RoomId))]
    [Index(nameof(LinkedId), nameof(RoomId))]
    public class MessageSent
    {
        public Guid Id { get; set; }

        public long RoomId { get; set; }
        public virtual Room Room { get; set; }

        public long? SenderId { get; set; }
        public virtual User Sender { get; set; }

        public long MessageTagId { get; set; }
        public virtual MessageTag MessageTag { get; set; }

        public MessageTypeEnum Type { get; set; }

        public long? LinkedId { get; set; }
        public virtual MessageSent Linked { get; set; }

        public long? AuthorId { get; set; }
        public virtual User Author { get; set; }

        public MessageStatusEnum Status { get; set; }
        public long? FileId { get; set; }
        public virtual File File { get; set; }

        public long URLPreviewId { get; set; }
        public virtual URLPreview URLPreview { get; set; }

        public long DateDrafted { get; set;}
        public long DateScheduled { get; set; }


        [MaxLength(16383)]
        public string Body { get; set; }

        public long DateUserSent { get; set; }
        public long? DateServerReceived { get; set; }
        public long? DateServerSent { get; set; }
        public long? DateUpdated { get; set; }
        public long? DateDeleted { get; set; }
        public long? DateStarred { get; set; }
    }
}
