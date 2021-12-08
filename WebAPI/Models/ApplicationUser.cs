using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        [MaxLength(256)]
        public override string PasswordHash { get; set; }

        [MaxLength(256)]
        public override string SecurityStamp { get; set; }

        [MaxLength(256)]
        public override string ConcurrencyStamp { get; set; }

        [MaxLength(256)]
        public override string PhoneNumber { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateDeleted { get; set; }
    }

    public class ApplicationRole : IdentityRole<long>
    {
        [MaxLength(256)]
        public override string ConcurrencyStamp { get; set; }
    }
}
