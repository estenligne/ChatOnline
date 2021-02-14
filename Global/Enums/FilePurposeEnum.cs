namespace Global.Enums
{
    public enum FilePurposeEnum
    {
        None = 0x00,

        UserProfilePhoto = 0x02,
        GroupProfilePhoto = 0x03,
        UserProfileWallpaper = 0x04,
        GroupProfileWallpaper = 0x04,

        ChatRoomVoice = 0x10,
        ChatRoomImage = 0x11,
        ChatRoomAudio = 0x12,
        ChatRoomVideo = 0x13,
        ChatRoomDocument = 0x14, // files with preview available, such as PDF, DOCX
        ChatRoomFile = 0x15,
    }
}
