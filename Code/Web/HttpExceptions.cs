using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Web
{
    public class HttpException : Exception
    {
        public readonly int HttpCode;

        public HttpException(HttpStatusCode status)
        {
            this.HttpCode = (int)status;
        }

        public HttpException(HttpStatusCode status, string message)
            : base(message)
        {
            this.HttpCode = (int)status;
        }

        static public HttpException Unauthorized()
        {
            return new HttpException(HttpStatusCode.Unauthorized, "UNAUTHORIZED");
        }

        static public HttpException NotFound()
        {
            return new HttpException(HttpStatusCode.NotFound, "NOT_FOUND");
        }

        static public HttpException NotAllowed()
        {
            return new HttpException(HttpStatusCode.MethodNotAllowed, "MENTHOD_NOT_ALLOWED");
        }

        static public HttpException Forbidden()
        {
            return new HttpException(HttpStatusCode.Forbidden, "FORBIDDEN");
        }
    }

    public class HttpUserException : HttpException
    {
        public readonly string Code;

        public HttpUserException(string code, string message)
            : base(HttpStatusCode.BadRequest, message)
        {
            this.Code = code;
        }
    }
}
