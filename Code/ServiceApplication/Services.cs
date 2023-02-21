using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SoulFab.Core.Web;
using System;
using System.Runtime.CompilerServices;

namespace SoulFab.Core.System
{

    internal static class ServiceExtension
    {
        static public void HostConfigure(this IHostBuilder host, ISystem system)
        {
            if (!system.InConsole)
            {
                host.UseWindowsService()
               .ConfigureAppConfiguration((_, config) =>
               {
                   config.AddJsonFile(system.RootPath + "appsettings.json");
               });
            }

            /*
            host.ConfigureLogging(builder =>
                builder.ClearProviders().AddProvider(system.Get<ILoggerProvider>())
            );
            */
        }

        static public void ServicesConfig(this IServiceCollection services, ISystem system)
        {
            services.AddSingleton(system);
            services.AddHostedService<ServiceWorker>();
        }
    }

    public class WebService6
    {
        public static void Run(string[] args, ISystem system)
        {
            WebApplicationOptions options = new()
            {
                ContentRootPath = AppContext.BaseDirectory,
                Args = args,
            };

            var builder = WebApplication.CreateBuilder(options);

            builder.Host.HostConfigure(system);
            builder.Services.ServicesConfig(system);

            builder.Services.WebServiceConfigure(system);
            builder.WebHost.WebHostConfig(system);

            var app = builder.Build();

            app.WebAppConfigure(system, app.Environment);

            app.Run();
        }
    }

    public class WebService2
    {
        public static void Run(string[] args, ISystem system)
        {
            var builder = new WebHostBuilder();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton(system);
            });

            builder.WebHostConfig(system);
            builder.UseStartup<Startup>();

            var host = builder.Build();
            if (!system.InConsole)
            {
                host.Run();
            }
            else
            {
                // host.RunAsService();
            }
        }
    }

    public class WebAPIService
    {
        public static void Run(string[] args, ISystem system)
        {
            var host = Host.CreateDefaultBuilder(args);

            host.HostConfigure(system);

            var app = host.ConfigureWebHostDefaults(host =>
            {
                host.WebHostConfig(system);

                host.ConfigureServices(services =>
                {
                    services.ServicesConfig(system);
                    services.WebServiceConfigure(system);
                });

                host.Configure(builder =>
                {
                    builder.WebAppConfigure(system, null);
                });
            }).Build();

            app.Run();
        }
    }
}
