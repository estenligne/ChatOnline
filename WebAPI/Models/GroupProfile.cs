using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.GroupProfiles))]
    [Index(nameof(CreatorId), nameof(GroupName), IsUnique = true)]
    public class GroupProfile
    {
        public long Id { get; set; }

        public long CreatorId { get; set; }
        public virtual UserProfile Creator { get; set; }

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

        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}
