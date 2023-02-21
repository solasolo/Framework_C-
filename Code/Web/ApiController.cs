using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoulFab.Core.System;

namespace SoulFab.Core.Web
{
    public interface IJWTRequired
    {
        bool JWTCheck();
    }

    public abstract class ApiController : ControllerBase
    {
        public abstract void setSystem(ISystem system);

        protected string ReadQuery(string name)
        {
            string ret = null;

            if (this.Request.Query.ContainsKey(name))
            {
                string text = this.Request.Query[name][0];

                ret = HttpUtility.UrlDecode(text.Trim());
            }

            return ret;
        }

        protected IActionResult JsonString(string json)
        {
            var ret = new ContentResult()
            {
                Content = json,
                ContentType = "application/json; charset=utf-8",
                StatusCode = 200,
            };
            

            return ret;
        }
    }

    public class ApiControllerActivator : IControllerActivator
    {
        public object Create(ControllerContext context)
        {
            var ControllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();
            var controller = context.HttpContext.RequestServices.GetRequiredService(ControllerType);

            if (controller is ApiController)
            {
                var system = context.HttpContext.RequestServices.GetRequiredService<ISystem>();
                (controller as ApiController).setSystem(system);
            }

            return controller;
        }

        public void Release(ControllerContext context, object controller)
        {

        }
    }

    public static class ApiControllerExtensions
    {
        static void ApiControllerServiceProvider(this IMvcBuilder mvcBuilder)
        {
            var services = mvcBuilder.Services;

            var serviceDescriptor = services.Where(m => m.ServiceType == typeof(IControllerActivator)).First();
            var existedServiceType = serviceDescriptor.ImplementationType;

            services.Add(ServiceDescriptor.Describe(existedServiceType, existedServiceType, serviceDescriptor.Lifetime));
            services.Replace(ServiceDescriptor.Describe(typeof(IControllerActivator), typeof(ApiControllerActivator), serviceDescriptor.Lifetime));
        }
    }

    /*
    public class ApiControllersResoler : IAssembliesResolver
    { 
    }
    */
}
