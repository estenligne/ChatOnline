using System;
using System.ComponentModel.DataAnnotations;
using Global.Enums;

namespace Global.Models
{
    public class DeviceUsedDTO
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public UserProfileDTO UserProfile { get; set; }

        public DevicePlatformEnum Platform { get; set; }

        [StringLength(63)]
        public string Language { get; set; }

        [StringLength(63)]
        public string Timezone { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }

        //public string PushNotificationToken { get; set; }
        public DateTimeOffset? DateTokenProvided { get; set; }
    }
}
