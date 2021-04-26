using System;
using System.ComponentModel.DataAnnotations;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public long Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Password { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Used only on Login
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Used only on Register
        /// </summary>
        [StringLength(63)]
        public string ProfileName { get; set; }
    }
}
