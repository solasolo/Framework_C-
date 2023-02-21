using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public abstract class UDPConnection<T> : BaseFrameChannel<T>
    {
        protected Socket InnerSocket;
        protected IPEndPoint LocalEndPoint;
        protected IPEndPoint RemoteEndPoint;

        public UDPConnection(string ip, int port)
        {
            this.LocalEndPoint = BaseSocket.MakeEndPoint(ip, port);
            this.ResetConnection();
        }

        public UDPConnection(IPAddress ip, int port)
        {
            this.LocalEndPoint = new IPEndPoint(ip, port);
            this.ResetConnection();
        }

        public UDPConnection(int port)
        {
            this.LocalEndPoint = null;

            this.RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            this.ResetConnection();
        }

        public void SetRemote(string ip, int port)
        {
            this.RemoteEndPoint = BaseSocket.MakeEndPoint(ip, port);
        }

        public override void Send(byte[] data)
        {
            this.InnerSocket.SendTo(data, this.RemoteEndPoint);
        }

        protected override int Receive(ref byte[] buf)
        {
            int ret = 0;

            EndPoint end_point = this.LocalEndPoint;
            ret = this.InnerSocket.Available;
            if(ret > 0)
            {
                buf = new byte[ret];
                ret = this.InnerSocket.ReceiveFrom(buf, ref end_point);
            }

            return ret;
        }

        protected override void Release()
        {
            if (InnerSocket != null)
            {
                try
                {
                    InnerSocket.Shutdown(SocketShutdown.Both);
                    InnerSocket.Close();
                }
                catch (Exception ex)
                {
                    try
                    {
                        this.Logger?.Error(ex, "Close Socker");
                    }
                    catch (Exception)
                    {
                    }
                }

                InnerSocket = null;
            }
        }

        protected override void ResetConnection()
        {
            this.Release();

            this.InnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (this.LocalEndPoint != null)
            {
                this.InnerSocket.Bind(this.LocalEndPoint);
            }
            else 
            {
                this.InnerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }

            this._IsOnline = true;
        }
    }

    public abstract class SimpleUDPConnection : UDPConnection<short>
    {
        private IChannelCallback<short> Callback = null;
        protected IMessageCodec<short> MessageCodec;

        public SimpleUDPConnection(string host, int port)
            : base(host, port)
        {
            this.MakeCodec();
        }

        public SimpleUDPConnection(IPAddress host, int port)
            : base(host, port)
        {
            this.MakeCodec();
        }

        public SimpleUDPConnection(int port)
            : base(port)
        {
            this.MakeCodec();
        }

        public void setCallback(IChannelCallback<short> callback)
        {
            this.Callback = callback;
        }

        public void setMessageCodec(IMessageCodec<short> codec)
        {
            this.MessageCodec = codec;
        }

        private void MakeCodec()
        {
            var codec = new SimpleFrameCodec();
            this.SetFrameCodec(codec);

            this.MessageCodec = new SimpleMessageCodec();
        }
    }
}
