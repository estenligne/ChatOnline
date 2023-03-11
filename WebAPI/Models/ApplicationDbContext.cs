using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<File> Files { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<UserRoom> UserRooms { get; set; }
        public DbSet<MessageTag> MessageTags { get; set; }
        public DbSet<MessageSent> MessagesSent { get; set; }
        public DbSet<MessageReceived> MessagesReceived { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<URLPreview> URLPreviews { get; set; }
    }
}
