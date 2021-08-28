using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<GroupProfile> GroupProfiles { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<UserChatRoom> UserChatRooms { get; set; }
        public DbSet<MessageTag> MessageTags { get; set; }
        public DbSet<MessageSent> MessagesSent { get; set; }
        public DbSet<MessageReceived> MessagesReceived { get; set; }
        public DbSet<DeviceUsed> DevicesUsed { get; set; }
    }
}
