using System;
using System.ComponentModel.DataAnnotations;

namespace Global.Models
{
    public class GroupProfileDTO
    {
        public long Id { get; set; }

        public long CreatorId { get; set; }

        [StringLength(63)]
        public string GroupName { get; set; }

        [StringLength(63)]
        public string JoinToken { get; set; }

        [StringLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public FileDTO PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public FileDTO WallpaperFile { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
    }
}
