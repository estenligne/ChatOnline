using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.GroupProfiles))]
    public class GroupProfile
    {
        [Key, ForeignKey(nameof(ChatRoom))]
        public long ChatRoomId { get; set; }

        [Required]
        [MaxLength(63)]
        public string GroupName { get; set; }

        [MaxLength(63)]
        public string JoinToken { get; set; }

        [MaxLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual File PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual File WallpaperFile { get; set; }
    }
}
