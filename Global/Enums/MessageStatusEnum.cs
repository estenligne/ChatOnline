using System;
using System.Collections.Generic;
using System.Text;

namespace Global.Enums
{
    public enum MessageStatusEnum
    {
        None = 0,
        Draft = 1,
        Scheduled = 2,
        UserSent = 3,
        ServerReceived = 4,
        ServerSent = 5,
        Received = 6,
        Read = 7,
        Downloaded = 8,
        FileOpened = 9,
    }
}
