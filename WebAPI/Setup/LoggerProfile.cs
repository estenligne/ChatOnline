using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace WebAPI.Setup
{
    public static class LoggerProfile
    {
        public enum Kind
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
            else return Kind.NLog;
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                if (GetKind() == Kind.NLog)
                {
                    builder.AddNLog("nlog.config");
                }
                else builder.AddConsole();
            });
        }

        public static ILogger GetLogger<T>()
        {
            return GetLoggerFactory().CreateLogger<T>();
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
