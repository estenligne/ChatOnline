using System;
using Global.Enums;

namespace Global.Models
{
    public class PushNotificationDTO
    {
        #region Generic to all push notifications

        public PushNotificationTopic Topic { get; set; }

        public int Id { get; set; } // used to identify a notification
        public string Tag { get; set; } // used to update a notification

        public string Title { get; set; }
        public string Body { get; set; }

        public bool Priority { get; set; } // true if a priority notification
        public DateTimeOffset DateCreated { get; set; }

        public static string GetChannelId(PushNotificationTopic topic)
        {
            return
                topic == PushNotificationTopic.MessageSent ? "chatonline_message_received" :
                "default_notification_channel_id";
        }

        public static string GetSoundFile(PushNotificationTopic topic)
        {
            return
                topic == PushNotificationTopic.MessageSent ? "default" :
                "default";
        }

        #endregion

        #region Used for message sent, received and read

        /// <summary>
        /// Interpretation is based on which Topic:
        /// if topic == message sent then this is the message sender's id,
        /// if topic == message received or read then this is the receiver's id.
        /// </summary>
        public long UserChatRoomId { get; set; }

        public long MessageSentId { get; set; }

        #endregion
    }
}
