using Global.Enums;

namespace Global.Models
{
    /// <summary>
    /// ChatRoom information to be shown on the main page of the mobile app where a list of all chat rooms is presented.
    /// </summary>
    public class ChatRoomInfo
    {
        /// <summary>
        /// The ChatRoom.Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The ChatRoom.Type
        /// </summary>
        public ChatRoomTypeEnum Type { get; set; }

        /// <summary>
        /// The UserProfile.Username or GroupProfile.GroupName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The PhotoFile.Name of the UserProfile or GroupProfile
        /// </summary>
        public string PhotoFileName { get; set; }

        /// <summary>
        /// The logged-in user's UserChatRoom.Id
        /// </summary>
        public long UserChatRoomId { get; set; }

        /// <summary>
        /// True if UserChatRoom.DateBlocked != null
        /// </summary>
        public bool UserBlocked { get; set; }

        /// <summary>
        /// True if UserChatRoom.DateExited != null
        /// </summary>
        public bool UserExited { get; set; }

        /// <summary>
        /// True if UserChatRoom.DateMuted + UserChatRoom.MuteDuration > DateTime.UtcNow
        /// </summary>
        public bool UserOnMute { get; set; }

        /// <summary>
        /// Information about the latest message sent in the chat room.
        /// </summary>
        public MessageShortInfo LatestMessage { get; } = new MessageShortInfo();
    }
}
