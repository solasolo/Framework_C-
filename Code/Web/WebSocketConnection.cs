using SoulFab.Core.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace SoulFab.Core.Web
{
    public class WebSocketConnection
    {
        public readonly string Id;

        WebSocketServer Server;
        WebSocket InnerSocket;

        IFrameCodec<short> FrameCodec;

        public WebSocketConnection(WebSocketServer server, string id, WebSocket socket)
        {
            this.Server = server;
            this.Id = id;
            this.InnerSocket = socket;

            this.FrameCodec = new SimpleFrameCodec();
        }

        public async Task Send(byte[] data)
        {
            try
            {
                await this.InnerSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.Server.ClientError(this, ex);
            }
        }

        public async Task Send(string str) 
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);

            try
            {
                await this.InnerSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                this.Server.ClientError(this, ex);
            }

        }

        public async Task ReceiveLoop(CancellationToken cancel)
        {
            while (this.InnerSocket.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = new ArraySegment<Byte>(new Byte[4096]);
                    var received = await this.InnerSocket.ReceiveAsync(buffer, cancel);

                    this.Server.ClientMessage(this, received.MessageType, buffer);
                }
                catch (WebSocketException ex)
                {
                    this.Server.ClientError(this, ex);

                    break;
                }
            }
        }
    }
}
