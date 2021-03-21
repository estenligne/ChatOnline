using System;
using SQLite;

namespace XamApp.Models
{
    public class User
    {
        [PrimaryKey]
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
}
