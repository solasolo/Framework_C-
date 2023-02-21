using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using SoulFab.Core.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoulFab.Core.Web
{
    class WebApiExceptionFilter : IAsyncExceptionFilter
    {
        private ILogger Logger;

        public WebApiExceptionFilter(ISystem system)
        {
            this.Logger = system.Get<ILogger>();
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // this.Logger.Error(context.Exception, "API Error");
        }
    }


    public class ExceptionHandlerMiddleware
    {
        private ILogger Logger;
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next, ISystem system)
        {
            _next = next;
            this.Logger = system.Get<ILogger>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this._next(context);
            }
            catch (HttpUserException ex)
            {
                context.Response.StatusCode = 400;
                var json = JsonSerializer.Serialize(new { Code = ex.Code, Message = ex.Message });

                await context.Response.WriteAsync(json);
            }
            catch (HttpException ex)
            {
                context.Response.StatusCode = ex.HttpCode;

                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(ex.Message);

                this.Logger.Error(ex);
            }

            return;
        }
    }

    static public class ExceptionHandlerExtensions
    {
        static public IApplicationBuilder UseExceptionHandler(this IApplicationBuilder builder)
        {
          return builder.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}
