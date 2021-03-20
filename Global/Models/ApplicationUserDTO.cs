using System;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public bool RememberMe { get; set; }
    }
}
