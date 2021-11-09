using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string loggerType = Environment.GetEnvironmentVariable("ASPNETCORE_LOGGER");
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                if (loggerType == "NLog")
                {
                    builder.AddNLog("nlog.config");
                }
                else builder.AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("---------------------------------");

                IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
                hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

                if (loggerType == "NLog")
                {
                    hostBuilder.ConfigureLogging(logging => logging.ClearProviders());
                    hostBuilder.UseNLog();
                }

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
                if (loggerType == "NLog")
                {
                    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                    NLog.LogManager.Shutdown();
                }
            }
        }
    }
}
