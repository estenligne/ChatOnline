using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.NewModels
{
    [Index(nameof(Name))]
    public class UserProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [MaxLength(63)]
        public string Name { get; set; }

        [MaxLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual FileModel PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual FileModel WallpaperFile { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        public DateTimeOffset LastConnected { get; set; }
        public AvailabilityEnum Availability { get; set; }
    }
}
