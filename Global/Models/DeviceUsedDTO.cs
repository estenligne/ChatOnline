using System;
using Global.Enums;

namespace Global.Models
{
    public class DeviceUsedDTO
    {
        public long Id { get; set; }

        public long UserProfileId { get; set; }
        public UserProfileDTO UserProfile { get; set; }

        public DevicePlatformEnum DevicePlatform { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }

        public string PushNotificationToken { get; set; }
        public DateTime? DateTokenProvided { get; set; }
    }
}
