using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.UserProfiles))]
    [Index(nameof(Identity), IsUnique = true)]
    public class UserProfile
    {
        public long Id { get; set; }

        /// <summary>
        /// The unique intelligible identifier of a user's account.
        /// This corresponds to the Account.AspNetUsers.UserName value,
        /// which in turn is equal to the email address or phone number.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Identity { get; set; }

        [MaxLength(63)]
        public string Username { get; set; }

        [MaxLength(4095)]
        public string About { get; set; }

        public AvailabilityEnum Availability { get; set; }

        public long? PhotoFileId { get; set; }
        public virtual File PhotoFile { get; set; }

        public long? WallpaperFileId { get; set; }
        public virtual File WallpaperFile { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
    }
}
