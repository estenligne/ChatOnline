using System;
using System.ComponentModel.DataAnnotations;

namespace Global.Models
{
    public class MessageTagDTO
    {
        public long Id { get; set; }

        [StringLength(63)]
        public string Name { get; set; }

        public long ChatRoomId { get; set; }

        public long? ParentId { get; set; }

        public long CreatorId { get; set; }

        public DateTimeOffset DateCreated { get; set; }
    }
}
