using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebAPI.Models
{
    public static class ApplicationDbSeed
    {
        /// <summary>
        /// Seed the database
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public static void Seed(ApplicationDbContext context, ILogger logger)
        {
            if (context.Database.EnsureCreated())
                logger.LogWarning("Database Created");

            context.Database.Migrate();
        }
    }
}
