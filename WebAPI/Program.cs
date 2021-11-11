using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Setup;
using System;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ILogger logger = LoggerProfile.GetLoggerFactory().CreateLogger<Program>();
            try
            {
                logger.LogInformation("---------------------------------");

                IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

                LoggerProfile.ConfigureLogging(hostBuilder);

                IHost host = hostBuilder.Build();

                Models.ApplicationDbSeed.Initialize(host);

                host.Run();
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, null);
                throw;
            }
            finally
            {
                LoggerProfile.Shutdown();
            }
        }
    }
}
