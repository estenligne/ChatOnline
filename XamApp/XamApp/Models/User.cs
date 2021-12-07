using System;
using SQLite;

namespace XamApp.Models
{
    public class User
    {
        [PrimaryKey]
        public long Id { get; set; }
        public long DeviceUsedId { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public bool RememberMe { get; set; }
        public string Authorization { get; set; }
    }
}
