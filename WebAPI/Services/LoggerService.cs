using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace WebAPI.Services
{
    public static class LoggerService
    {
        private enum Kind
        {
            Console,
            NLog,
        }

        private static Kind GetKind()
        {
            string value = Environment.GetEnvironmentVariable("ASPNETCORE_LOGGER");

            if (!string.IsNullOrEmpty(value))
            {
                return Enum.Parse<Kind>(value);
            }
            else return Kind.Console;
        }

        private static ILoggerFactory loggerFactory = null;

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger(string categoryName)
        {
            if (loggerFactory == null)
            {
                loggerFactory = LoggerFactory.Create(builder =>
                {
                    if (GetKind() == Kind.NLog)
                    {
                        builder.AddNLog("nlog.config");
                    }
                    else builder.AddConsole();
                });
            }
            return loggerFactory.CreateLogger(categoryName);
        }

        public static void ConfigureLogging(IHostBuilder hostBuilder)
        {
            if (GetKind() == Kind.NLog)
            {
                hostBuilder.ConfigureLogging(logging => logging.ClearProviders());
                hostBuilder.UseNLog();
            }
        }

        public static void Shutdown()
        {
            if (GetKind() == Kind.NLog)
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
    }
}
