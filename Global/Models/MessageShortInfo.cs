using System;

namespace Global.Models
{
    public class MessageShortInfo
    {
        /// <summary>
        /// The MessageSent.Id
        /// </summary>
        public long Id { get; set; }

        public long SenderId { get; set; }

        public string ShortBody { get; set; }

        public DateTimeOffset DateSent { get; set; }

        /// <summary>
        /// Number of users that have not yet received the message sent
        /// </summary>
        public int NotReceivedCount { get; set; }

        /// <summary>
        /// Number of users that have not yet read the message sent
        /// </summary>
        public int NotReadCount { get; set; }
    }
}
