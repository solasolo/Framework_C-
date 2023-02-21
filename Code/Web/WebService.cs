using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoulFab.Core.Config;
using SoulFab.Core.Helper;
using SoulFab.Core.Logger;
using SoulFab.Core.System;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.Web
{
    /*
    internal class WinWebHostService : WebHostService
    {
        public WinWebHostService(IWebHost host) : base(host)
        {
        }

        protected override void OnStarting(string[] args)
        {
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            base.OnStopping();
        }
    }
    */

    public interface IServiceRoute
    {
        void Route(IEndpointRouteBuilder builder);
    }

    public class ServiceWorker : BackgroundService
    {
        private readonly ILogger<ServiceWorker> _logger;

        private ILogger Logger;
        private ISystem System;
        private IService Service;

        public ServiceWorker(ISystem system)
            : base()
        {
            this.System = system;
            this.Service = system as IService;
            this.Logger = this.System.Get<ILogger>();
        }

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            this.Logger.Info("Service Starting");
            this.Service?.Start();
            this.Logger.Info("Service Started");

            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // this.Logger.Debug($"Worker running at: {DateTimeOffset.Now}");
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.Logger.Info("Service Stopping");
            this.Service?.Stop();
            this.Logger.Info("Service Stoped");

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            this.Service?.Stop();
            this.Logger.Info("Service Dsposed");

            base.Dispose();
        }
    }

    static public class WebConfigExtension
    {
        static public void WebServiceConfigure(this IServiceCollection services, ISystem system)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(new JWTFilter());
                options.Filters.Add(new WebApiExceptionFilter(system));

            })
            .ConfigureApplicationPartManager(apm =>
            {
                ApplicationPartsConfig(apm, system);
            })
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
            .AddControllersAsServices();

            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ApiControllerActivator>());
        }

        static public void ApplicationPartsConfig(ApplicationPartManager manager, ISystem system)
        {
            if (system != null)
            {
                // var asms = ReflectHelper.LoadAssemblyFromPath(system.RootPath);
                var asms = AppDomain.CurrentDomain.GetAssemblies();

                var ControllerType = typeof(ControllerBase);

                foreach (var asm in asms)
                {
                    try
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            if (type.IsSubclassOf(ControllerType))
                            {
                                manager.ApplicationParts.Add(new AssemblyPart(asm));
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        static public void WebHostConfig(this IWebHostBuilder host, ISystem systen)
        {
            int port = 8088;

            var config = systen.Get<IConfig>();
            var web_conf = config?.GetConfig("WebService");
            if (web_conf != null)
            {
                port = web_conf.GetInt("Port");
            }

            host.UseIISIntegration();
            host.UseKestrel((_, config) =>
            {
                config.Listen(IPAddress.Any, port);
            });
        }

        static public void WebAppConfigure(this IApplicationBuilder builder, ISystem system, IWebHostEnvironment env)
        {
            if (env != null && env.IsDevelopment())
            {
                builder.UseDeveloperExceptionPage();
            }

            builder.UseExceptionHandler();

            builder.UseRouting();
            builder.UseWebSocketServer(system);

            if (system is IServiceRoute)
            {
                builder.UseEndpoints(endpoints => (system as IServiceRoute).Route(endpoints));
            }

            builder.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        /*
        public static void RunAsMyService(this IWebHost host)
        {
            var service = new WinWebHostService(host);
            ServiceBase.Run(service);
        }
        */
    }

    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISystem _system;

        public Startup(IConfiguration configuration, ISystem system, IWebHostEnvironment env)
        {
            _system = system;
            _env = env;
        }

        public void Configure(IApplicationBuilder builder, ISystem system)
        {
            builder.WebAppConfigure(system, _env);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.WebServiceConfigure(_system);
        }
    }
}