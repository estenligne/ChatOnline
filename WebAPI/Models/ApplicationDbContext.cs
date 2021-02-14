using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<File> Files { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GroupProfile> GroupProfiles { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<UserChatRoom> UserChatRooms { get; set; }
        public DbSet<MessageTag> MessageTags { get; set; }
        public DbSet<MessageSent> MessagesSent { get; set; }
        public DbSet<MessageReceived> MessagesReceived { get; set; }
    }
}
