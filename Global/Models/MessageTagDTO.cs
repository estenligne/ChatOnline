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

        public long CreatorId { get; set; }

        public DateTime DateCreated { get; set; }

        public bool IsPrivate { get; set; }
    }
}
