using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAPI.Models
{
    public static class ApplicationDbSeed
    {
        // https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/working-with-sql?view=aspnetcore-5.0&tabs=visual-studio#seed-the-database
        public static void Initialize(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                using (var dbc = services.GetRequiredService<ApplicationDbContext>())
                {
                    dbc.Database.Migrate();
                }
            }
        }
    }
}
