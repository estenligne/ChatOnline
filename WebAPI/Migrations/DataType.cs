using System.Runtime.InteropServices;

namespace WebAPI.Migrations
{
    /// <summary>
    /// Data types set based on which DBMS is used
    /// </summary>
    public static class DataType
    {
        public static bool UseMySQL => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static string DateTime => UseMySQL ? "datetime" : "datetime2";
        public static string Timestamp => UseMySQL ? "timestamp" : "datetimeoffset";
        public static string NVarCharMax => UseMySQL ? "nvarchar(16383)" : "nvarchar(max)";
    }
}
