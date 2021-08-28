using System;
using System.ComponentModel.DataAnnotations;

namespace Global.Models
{
    public class UserProfileDTO
    {
        public long Id { get; set; }

        public string Identity { get; set; }

        [StringLength(63)]
        public string Username { get; set; }

        [StringLength(63)]
        public string Availability { get; set; }

        [StringLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public FileDTO PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public FileDTO WallpaperFile { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
