using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Global.Enums;

namespace WebAPI.Models
{
    [Table(nameof(ApplicationDbContext.DevicesUsed))]
    public class DeviceUsed
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; }

        public DevicePlatformEnum DevicePlatform { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }

        [MaxLength(1023)]
        public string PushNotificationToken { get; set; }
        public DateTime? DateTokenProvided { get; set; }
    }
}
