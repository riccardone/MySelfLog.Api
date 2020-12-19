using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Logging;
using NLog.Web;

namespace MySelfLog.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            var projectName = Assembly.GetCallingAssembly().GetName().Name;

            try
            {
                logger.Info($"App start: {projectName}");
                IdentityModelEventSource.ShowPII = true;
                CreateHostBuilder(args, env).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Stopped program {projectName} because of exception in startup");
                throw;
            }
            finally
            {
                logger.Warn($"App Stopping: {projectName}");
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args, string env) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    c.AddJsonFile("config/myselflog-api-multi-tenancy-config-map.json", optional: true, reloadOnChange: true); // for config maps loaded into AKS
                    c.AddEnvironmentVariables();

                    if (!string.IsNullOrWhiteSpace(env) && env == "Development")
                        c.AddJsonFile($"appsettings.{env}.json", optional: true);

                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                })
                .UseNLog()
                .UseStartup<Startup>();
    }
}
