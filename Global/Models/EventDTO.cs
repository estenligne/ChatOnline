using System;
using Global.Enums;

namespace Global.Models
{
    public class EventDTO
    {
        public long Id { get; set; }

        public ChatRoomEventEnum Event { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateOccurred { get; set; }
    }
}
