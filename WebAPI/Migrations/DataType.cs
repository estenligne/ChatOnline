﻿using System;

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

        private static DBMS GetDBMS()
        {
            if (dbms == DBMS.Unknown)
            {
                string value = Environment.GetEnvironmentVariable("ASPNETCORE_DBMS");
                Enum.TryParse<DBMS>(value, out dbms);

                if (dbms == DBMS.Unknown)
                    dbms = DBMS.SQLite;
            }
            return dbms;
        }

        public static bool UseSQLServer => GetDBMS() == DBMS.SQLServer;
        public static bool UseSQLite => GetDBMS() == DBMS.SQLite;
        public static bool UseMySQL => GetDBMS() == DBMS.MySQL;

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
