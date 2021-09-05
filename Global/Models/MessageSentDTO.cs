using System;
using System.ComponentModel.DataAnnotations;
using Global.Enums;

namespace Global.Models
{
    public class MessageSentDTO
    {
        public long Id { get; set; }

        /// <summary>
        /// A FK to a UserChatRoom, for the user who sent the message.
        /// </summary>
        public long SenderId { get; set; }

        public string SenderName { get; set; }

        /// <summary>
        /// A FK to a UserChatRoom. Is equal to 0 if SenderId == signed-in user.
        /// </summary>
        public long ReceiverId { get; set; }

        public long MessageTagId { get; set; }
        public MessageTagDTO MessageTag { get; set; }

        public MessageTypeEnum MessageType { get; set; }

        /// <summary>
        /// A FK to a MessageSent. It is the message being replied to or being forwarded.
        /// </summary>
        public long? LinkedId { get; set; }

        /// <summary>
        /// If AuthorId != null then message is a <b>forwarded</b> message specified by LinkedId.
        /// </summary>
        public long? AuthorId { get; set; }

        public long? FileId { get; set; }
        public FileDTO File { get; set; }

        [StringLength(16383)]
        public string Body { get; set; }

        /// <summary>
        /// Equal to MessageSent.DateSent, even if not SenderId == signed-in user
        /// </summary>
        public DateTimeOffset DateSent { get; set; }

        /// <summary>
        /// Equal to null if SenderId == signed-in user, else equal to MessageReceived.DateReceived
        /// </summary>
        public DateTimeOffset? DateReceived { get; set; }

        /// <summary>
        /// Equal to null if SenderId == signed-in user, else equal to MessageReceived.DateRead
        /// </summary>
        public DateTimeOffset? DateRead { get; set; }

        /// <summary>
        /// Equal to MessageSent.DateDeleted if SenderId == signed-in user, else equal to MessageReceived.DateDeleted
        /// </summary>
        public DateTimeOffset? DateDeleted { get; set; }

        /// <summary>
        /// Equal to MessageSent.DateStarred if SenderId == signed-in user, else equal to MessageReceived.DateStarred
        /// </summary>
        public DateTimeOffset? DateStarred { get; set; }

        /// <summary>
        /// Equal to None if SenderId == signed-in user, else equal to MessageReceived.Reaction
        /// </summary>
        public MessageReactionEnum Reaction { get; set; }
    }
}
