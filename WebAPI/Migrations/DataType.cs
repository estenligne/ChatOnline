using System.Runtime.InteropServices;

namespace WebAPI.Migrations
{
    /// <summary>
    /// Data types set based on which DBMS is used
    /// </summary>
    public static class DataType
    {
        public static bool UseMySQL => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string Bool => UseMySQL ? "tinyint(1)" : "bit";

        public static string DateTime => UseMySQL ? "datetime" : "datetime2";

        public static string Timestamp => UseMySQL ? "timestamp" : "datetimeoffset";

        public static string String(int maxLength)
        {
            if (maxLength == -1)
            {
                return UseMySQL ? "text" : "nvarchar(max)";
            }
            else if (UseMySQL)
            {
                return maxLength == 450 ?
                    $"varchar(767)" :
                    $"varchar({maxLength})";
            }
            else return $"nvarchar({maxLength})";
        }
    }
}
