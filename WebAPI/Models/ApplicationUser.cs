using System;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
    }

    public class ApplicationRole : IdentityRole<long>
    {
    }
}
