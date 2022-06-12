using System;

namespace WebAPI.Migrations
{
    /// <summary>
    /// Data types set based on which DBMS is used
    /// </summary>
    public static class DataType
    {
        private enum DBMS
        {
            Unknown,
            SQLServer,
            SQLite,
            MySQL,
        }

        private static DBMS dbms;

        public static void SetDBMS(string value)
        {
            if (!Enum.TryParse(value, out dbms))
            {
                throw new SystemException("DBMS not specified or not valid.");
            }
        }

        public static bool UseSQLServer => dbms == DBMS.SQLServer;
        public static bool UseSQLite => dbms == DBMS.SQLite;
        public static bool UseMySQL => dbms == DBMS.MySQL;

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
