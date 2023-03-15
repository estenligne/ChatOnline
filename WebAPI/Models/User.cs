using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Index(nameof(Name))]
    [Index(nameof(AccountId), IsUnique = true)]
    [Index(nameof(PhotoFileId), IsUnique = true)]
    [Index(nameof(WallpaperFileId), IsUnique = true)]
    public class User
    {
        public long Id { get; set; }

        public long? AccountId { get; set; }

        [Required]
        [MaxLength(63)]
        public string Name { get; set; }

        [MaxLength(4000)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual File PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual File WallpaperFile { get; set; }

        public long DateCreated { get; set; }
        public long? DateUpdated { get; set; }
        public long? DateDeleted { get; set; }

        public long DateLastOnline { get; set; }
        public AvailabilityEnum Availability { get; set; }
    }
}
