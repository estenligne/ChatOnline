using System;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string PhoneNumber { get; set; }
    }
}
