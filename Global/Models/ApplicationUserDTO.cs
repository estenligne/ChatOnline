using System;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Used only on GetUser()
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Used only on Login
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Used only on Register
        /// </summary>
        public string ProfileName { get; set; }
    }
}
