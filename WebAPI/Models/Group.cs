using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    [Index(nameof(PhotoFileId), nameof(WallpaperFileId), IsUnique =true)]
    public class Group
    {
        [Key, ForeignKey(nameof(Room))]
        public long RoomId { get; set; }

        [Required]
        [MaxLength(63)]
        public string Name { get; set; }

        [NotMapped]
        [MaxLength(63)]
        public string JoinToken { get; set; }

        [MaxLength(4000)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual File File { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual File WallpaperFile { get; set; }
    }
}
