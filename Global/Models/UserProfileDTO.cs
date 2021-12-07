using System;
using System.ComponentModel.DataAnnotations;
using Global.Enums;

namespace Global.Models
{
    public class UserProfileDTO
    {
        public long Id { get; set; }

        [Required]
        [StringLength(63)]
        public string Name { get; set; }

        [StringLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public FileDTO PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public FileDTO WallpaperFile { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        public DateTimeOffset LastConnected { get; set; }
        public AvailabilityEnum Availability { get; set; }
    }
}
