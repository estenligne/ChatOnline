using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebAPI.Models
{
    public static class ApplicationDbSeed
    {
        // https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/working-with-sql
        public static void Setup(IHost host, ILogger logger)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                using (var dbc = services.GetRequiredService<ApplicationDbContext>())
                {
                    dbc.Database.Migrate(); // must come first
                    Dummy(dbc, logger);
                }
            }
        }

        private static void Dummy(ApplicationDbContext dbc, ILogger logger)
        {
            try
            {
                dbc.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null);
            }
        }
    }
}
