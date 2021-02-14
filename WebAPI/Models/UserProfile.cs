using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.UserProfiles))]
    public class UserProfile
    {
        public long Id { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [MaxLength(63)]
        public string Username { get; set; }

        [MaxLength(63)]
        public string Availability { get; set; }

        [MaxLength(4095)]
        public string About { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual File PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual File WallpaperFile { get; set; }

        public DateTime? DateDeleted { get; set; }
    }
}
