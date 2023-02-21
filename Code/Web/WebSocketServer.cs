using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SoulFab.Core.Logger;
using Microsoft.AspNetCore.Builder;
using SoulFab.Core.System;
using System.Text.Json;
using System.Text;
using SoulFab.Core.Base;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Concurrent;

namespace SoulFab.Core.Web
{
    public interface IWebSocketHandler
    {
        Task Send(byte[] data);
        Task OnMessage(WebSocketConnection client, short cmd, byte[] Data);
    }

    public interface IWebSocketServer
    {
        Task Startup(HttpContext context);
        void Shutdown();
    }

    public abstract class WebSocketServer : IWebSocketServer
    {
        protected ILogger Logger;
        private ConcurrentDictionary<string, WebSocketConnection> ClientList;
        private CancellationTokenSource ShutdownSource;

        public WebSocketServer(ILogger logger)
        {
            this.Logger = logger;
            this.ClientList = new ConcurrentDictionary<string, WebSocketConnection>();

            this.ShutdownSource = new CancellationTokenSource();

        }

        public async Task Startup(HttpContext context)
        {
            var token = ShutdownSource.Token;

            this.HeartBeat(token);
            await this.Accept(context, token);
        }

        public void Shutdown()
        {
            this.ShutdownSource.Cancel();
        }

        public async Task Send<T>(int func, T obj)
        {
            string json = JsonSerializer.Serialize(obj);
            byte[] code_bytes = BitConverter.GetBytes(func);
            byte[] data_bytes = Encoding.UTF8.GetBytes(json);

            byte[] msg = new byte[data_bytes.Length + 4];
            code_bytes.CopyTo(msg, 0);
            data_bytes.CopyTo(msg, 4);

            foreach (var client in this.ClientList.Values)
            {
                await client.Send(msg);
            }
        }

        internal void ClientError(WebSocketConnection conn, Exception ex)
        {
            this.Logger.Error(ex, "Websocket Client Error");
            this.Remove(conn);
        }

        internal void Remove(WebSocketConnection conn)
        {
            WebSocketConnection item;
            this.ClientList.Remove(conn.Id, out item);
        }

        internal void ClientMessage(WebSocketConnection client, WebSocketMessageType type, ArraySegment<Byte> msg)
        {
            switch (type)
            {
                case WebSocketMessageType.Text:
                    var data = Encoding.UTF8.GetString(msg.Array, msg.Offset, msg.Count);
                    break;


                case WebSocketMessageType.Binary:
                    break;

                case WebSocketMessageType.Close:
                    break;
            }
        }

        private async Task Accept(HttpContext context, CancellationToken cancel)
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            string id = Guid.NewGuid().ToString(); ;
            var client = new WebSocketConnection(this, id, socket);
            try
            {
                this.ClientList.TryAdd(id, client);

                await client.ReceiveLoop(cancel);
            }
            catch (Exception ex)
            {
                this.Logger?.Error(ex, $"Error in Echo Websocket Client {id}");
                await context.Response.WriteAsync("closed");
            }
            finally
            {
                this.Remove(client);
            }
        }

        private Task HeartBeat(CancellationToken cancel)
        {
            return TimerTask.Run("Websocket Heart Beat", this.Logger, cancel, 1000, async () =>
            {
                await this.Send(0, DateTime.Now);
            });
        }

        protected abstract Task HandleMessage(WebSocketConnection client, short cmd, byte[] Data);
    }

    public static class WebSocketExtension
    {
        static public void UseWebSocketServer(this IApplicationBuilder builder, ISystem system)
        {
            builder.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(10),
            });

            builder.Use(async (context, next) =>
            {
                var SocketManage = context.WebSockets;

                if (SocketManage.IsWebSocketRequest)
                {
                    var server = system.Get<IWebSocketServer>();
                    await server.Startup(context);
                }
                else
                {
                    await next();
                }
            });
        }
    }
}
