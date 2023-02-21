using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SoulFab.Core.Base;
using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;

namespace SoulFab.Core.Communication
{
    class TCPServerConnection<T> : TCPConnection<T>
    {
        private TCPServer<T> Server;

        public TCPServerConnection(TCPServer<T> server, Socket socket)
        {
            Server = server;
            this.InnerSocket = socket;

            this._IsOnline = true;
        }

        protected override void ResetConnection()
        {
            this.InnerSocket.Close();
            this.Shutdown();

            Server.CleanSocket(this);

        }
        protected override void HandleFrameData(IChannel channel, MessageEnity<T> msg)
        {
            this.Server.RelayFrameData(channel, msg);
        }
    }

    public abstract class TCPServer<T>
    {
        private int ConnectRetryTime = 500;
        private bool Running;

        protected ILogger Logger;

        private Thread AcceptThread;
        private Socket InnerSocket;
        private IPEndPoint EndPoint;

        protected IFrameCodec<T> FrameCodec;

        private IList<TCPServerConnection<T>> Connections;

        public TCPServer(string host, int port)
            : base()
        {
            EndPoint = BaseSocket.MakeEndPoint(host, port);
            Connections = new ConcurrentList<TCPServerConnection<T>>();

            AcceptThread = new Thread(new ThreadStart(AcceptProc));
        }

        public void Startup()
        {
            InnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            InnerSocket.Bind(EndPoint);
            InnerSocket.Listen(10);

            Running = true;
            AcceptThread.Start();
        }

        public void Shutdown()
        {
            this.Running = false;
            this.InnerSocket.Close();

            foreach (var connection in this.Connections)
            {
                connection.Shutdown();
            }

            this.Connections.Clear();
        }

        public bool Connected
        {
            get
            {
                return Connections.Count > 0;
            }
        }

        public void SetFrameCodec(IFrameCodec<T> codec)
        {
            this.FrameCodec = codec;
        }

        public void Send(T cmd, byte[] msg)
        {
            foreach (var conn in this.Connections)
            {
                try
                {
                    conn.Send(cmd, msg);
                }
                catch (Exception ex)
                {
                    this.Logger?.Error(ex, "TCP Server Send");
                }
            }
        }

        public void setLogger(ILogger logger)
        {
            this.Logger = logger;
        }

        protected void AcceptProc()
        {
            while (Running)
            {
                Socket socket = InnerSocket.Accept();
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);

                var conn = new TCPServerConnection<T>(this, socket);
                conn.setLogger(this.Logger);
                conn.SetFrameCodec(this.FrameCodec);
                conn.Startup();

                Connections.Add(conn);

                this.HandleConnected(conn);
            }
        }

        protected abstract void HandleConnected(IChannel channel);
        protected abstract void HandleFrameData(IChannel channel, MessageEnity<T> msg);

        internal void CleanSocket(TCPServerConnection<T> conn)
        {
            if (conn != null)
            {
                Connections.Remove(conn);
            }
        }

        internal void RelayFrameData(IChannel channel, MessageEnity<T> msg)
        {
            this.HandleFrameData(channel, msg);
        }
    }
}
