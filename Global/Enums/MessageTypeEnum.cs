namespace Global.Enums
{
    /// <summary>
    /// Determines the way in which the message will be shown to the user.
    /// </summary>
    public enum MessageTypeEnum
    {
        Unknown = 0,
        Text,
        Html,
        Voice,
        Image,
        Audio,
        Video,
        Document,
        File,
        Contact, // special format inside body
        Location, // special format inside body
    }
}
