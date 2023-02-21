using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SoulFab.Core.Web
{
    public class JWTHeader
    {
        public int Id { get; set; }
        public long ExpireTime { get; set; }

        public string Type { get; set; }
        public string Algorithm { get; set; }

        public JWTHeader()
        { 
        }

        public JWTHeader(int mins)
        {
            this.Algorithm = "HS256";
            this.Type = "JWT";

            this.Id = Guid.NewGuid().GetHashCode();
            var t = DateTime.Now;
            t.AddMinutes(mins);

            this.ExpireTime = (t.Ticks - (new DateTime(1970, 1, 1)).Ticks) / TimeSpan.TicksPerMillisecond;
        }
    }

    static public class JWT
    {
        const string HEADER_NAME = "Authorization";
        const string TOKEN_PREFIX = "Bearer ";

        static public readonly byte[] SecurityKey;
        static public int ExpireTime;

        static JWT()
        {
            SecurityKey = new byte[32];

            for (int i = 0; i < 8; i++)
            {
                int v = Guid.NewGuid().GetHashCode();
                byte[] b = BitConverter.GetBytes(v);

                b.CopyTo(SecurityKey, i * 4);
            }

            ExpireTime = 640;
        }

        static public bool Check(HttpRequest req)
        {
            bool ret = false;

            var parts = JWT.Read(req);
            if (parts != null)
            {
                string sign = parts[2];

                var head_str = parts[0];
                if (sign == JWT.Sign(head_str, parts[1]))
                {
                    string json_head = Encoding.UTF8.GetString(Convert.FromBase64String(head_str));
                    var head = JsonSerializer.Deserialize<JWTHeader>(json_head);

                    if (head.ExpireTime < DateTime.Now.Ticks)
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        static public string CreateJWT<T>(this HttpRequest req, T payload)
        {
            var head = new JWTHeader(JWT.ExpireTime);
            string json_head = JsonSerializer.Serialize(head);
            string json_payload = JsonSerializer.Serialize(payload);

            var encoder = Encoding.UTF8;
            string head64 = Convert.ToBase64String(encoder.GetBytes(json_head));
            string payload64 = Convert.ToBase64String(encoder.GetBytes(json_payload));

            string sign = JWT.Sign(head64, payload64);

            return head64 + "." + payload64 + "." + sign;
        }

        static public T GetJWT<T>(this HttpRequest req)
        {
            T ret = default(T);

            var parts = JWT.Read(req);
            if (parts != null)
            {
                string json_payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                ret = JsonSerializer.Deserialize<T>(json_payload);
            }

            return ret;
        }

        static private string[] Read(HttpRequest req)
        {
            string[] ret = null;

            string token = null;

            var values = req.Headers[HEADER_NAME];
            foreach (var s in values)
            {
                if (s.StartsWith(TOKEN_PREFIX))
                {
                    token = s.Substring(TOKEN_PREFIX.Length);
                    break;
                }
            }

            if (!String.IsNullOrEmpty(token))
            {
                var parts = token.Split('.');
                if (parts.Length == 3)
                {
                    ret = parts;
                }
            }

            return ret;
        }

        static string Sign(string head, string payload)
        {
            string ret = "";

            using (var hasher = new HMACSHA512(JWT.SecurityKey))
            {
                var bs = Encoding.UTF8.GetBytes(head + payload);

                var bytes_sign = hasher.ComputeHash(bs);
                ret = Convert.ToBase64String(bytes_sign);
            }

            return ret;
        }
    }

    public class JWTFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller;
            if (controller is IJWTRequired)
            {
                (controller as IJWTRequired).JWTCheck();
            }
        }
    }

    public class TokenAuthentification
    {
        private readonly RequestDelegate _next;

        public TokenAuthentification(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (JWT.Check(context.Request))
            {
                await this._next(context);
            }
            else
            {
                throw HttpException.Unauthorized();
            }

            return;
        }
    }

    static public class JWTExtensions
    {
        static public IApplicationBuilder UseJWT(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenAuthentification>();
        }
    }
}
