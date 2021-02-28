namespace Global.Enums
{
    [System.Flags]
    public enum ChatRoomTypeEnum
    {
        Unknown = 0,
        Private = 1 << 0,
        Group = 1 << 1,
        Pinned = 1 << 2,
    }
}
