namespace Global.Enums
{
    /// <summary>
    /// Determines the way in which the message will be shown to the user.
    /// </summary>
    public enum MessageTypeEnum
    {
        MASK = 0xF0,
        Chat = 0x00,
        File = 0x10,
        Other = 0x20,
        Event = 0x30,

        Text = Chat + 1,
        Html = Chat + 2,
        Voice = Chat + 3,

        Image = File + 1,
        Audio = File + 2,
        Video = File + 3,
        Document = File + 4,
        SourceCode = File + 5,

        Contact = Other + 1,
        Location = Other + 2,
        Announcement = Other + 3,
        Advertisement = Other + 4,

        /// <summary>
        /// Marks a message as deleted when DateDeleted != null.
        /// Not stored in db, used only by the frontend.
        /// </summary>
        Deleted = Event + 1,

        /// <summary>
        /// Marks a switch in date between two messages.
        /// Not stored in db, used only by the frontend.
        /// </summary>
        SwitchInDate = Event + 2,
    }
}
