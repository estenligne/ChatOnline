using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Global.Enums;

namespace WebAPI.Models
{
    [Index(nameof(UserId))]
    public class UserDevice
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual User User { get; set; }

        public DevicePlatformEnum Platform { get; set; }

        [MaxLength(63)]
        public string Language { get; set; }

        [MaxLength(63)]
        public string Timezone { get; set; }

        public long DateCreated { get; set; }
        public long DateUpdated { get; set; }
        public long? DateDeleted { get; set; }

        [MaxLength(1023)]
        public string PushNotificationToken { get; set; }
        public long? DateTokenProvided { get; set; }

        public string LocalIPv4 { get; set; }
        public string RemoteIPv4 { get; set; }
    }
}
