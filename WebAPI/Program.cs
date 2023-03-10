using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Services;
using System;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ILogger logger = LoggerService.CreateLogger("Program");
            try
            {
                logger.LogInformation("--------------------");

                IHost host = CreateHostBuilder(args).Build();

                Models.ApplicationDbSeed.Setup(host, logger);

                host.Run();
            }
            catch (Exception exception)
            {
                logger.LogCritical(exception, null);
                throw;
            }
            finally
            {
                LoggerService.Shutdown();
            }
        }

        /// <summary>
        /// This is 'public static' as it is needed by Add-Migration.
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);

            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

            LoggerService.ConfigureLogging(hostBuilder);

            return hostBuilder;
        }
    }
}
