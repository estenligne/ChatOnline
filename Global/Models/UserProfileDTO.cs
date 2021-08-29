using System;
using System.ComponentModel.DataAnnotations;
using Global.Enums;

namespace Global.Models
{
    public class UserProfileDTO
    {
        public long Id { get; set; }

        public string Identity { get; set; }

        [StringLength(63)]
        public string Username { get; set; }

        [StringLength(4095)]
        public string About { get; set; }

        public AvailabilityEnum Availability { get; set; }

        public long? PhotoFileId { get; set; }
        public FileDTO PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public FileDTO WallpaperFile { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
    }
}
