using System;
using System.ComponentModel.DataAnnotations;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public long Id { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Used only on sign in
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
