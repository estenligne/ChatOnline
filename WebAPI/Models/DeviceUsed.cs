using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.DevicesUsed))]
    [Index(nameof(UserProfileId), nameof(Platform), IsUnique = true)]
    public class DeviceUsed
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        public DevicePlatformEnum Platform { get; set; }

        [MaxLength(63)]
        public string Language { get; set; }

        [MaxLength(63)]
        public string Timezone { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        [MaxLength(1023)]
        public string PushNotificationToken { get; set; }
        public DateTimeOffset? DateTokenProvided { get; set; }
    }
}
