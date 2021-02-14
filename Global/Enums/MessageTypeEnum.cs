namespace Global.Enums
{
    public enum MessageTypeEnum
    {
        Unknown = 0,
        Text,
        Voice,
        Image,
        Audio,
        Video,
        Document,
        File,
        Contact, // special format inside body
        Location, // special format inside body
        Forwarded,
    }
}
