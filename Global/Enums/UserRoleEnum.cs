namespace Global.Enums
{
    public enum UserRoleEnum
    {
        None = 0,
        FullNode = 1 << 0,
        Administrator = 1 << 1,
    }
}
