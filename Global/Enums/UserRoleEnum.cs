namespace Global.Enums
{
    [System.Flags]
    public enum UserRoleEnum
    {
        None = 0,
        FullNode = 1 << 0,
        GroupAdmin = 1 << 1,
    }
}
