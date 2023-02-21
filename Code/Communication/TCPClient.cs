using System;
using System.Net.Sockets;
using System.Threading;
using SoulFab.Core.Logger;

namespace SoulFab.Core.Communication
{
    public abstract class TCPClient<T> : TCPConnection<T>
    {
        protected int ConnectRetryTime = 5000;

        protected int HeartRate = 20;

        protected Thread WatchDogThread;

        public TCPClient(string host, int port)
            : base()
        {
            if (host == "0.0.0.0")
            {
                host = "127.0.0.1";
            }

            this.RemoteEndPoint = BaseSocket.MakeEndPoint(host, port);
            this.WatchDogThread = new Thread(new ThreadStart(WatchDogProc));
        }

        public override void Startup()
        {
            base.Startup();

            this.WatchDogThread.Start();
        }

        protected override void ResetConnection()
        {
            _IsOnline = false;

            while (!_IsOnline)
            {
                try
                {
                    Release();

                    this.InnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    this.InnerSocket.Connect(RemoteEndPoint);
                    this.HandleConnected(this);

                    _IsOnline = true;
                }
                catch (Exception ex)
                {
                    this.Logger?.Error(ex, "TCP Connect");
                }

                if (!_IsOnline)
                {
                    Thread.Sleep(ConnectRetryTime);
                }
            }
        }
        private void SendHearBeat()
        {
            if (this.IsOnline)
            {
                byte[] data = this.FrameCodec.EncodeHeartbeat();

                this.Send(data);
            }
        }

        private void WatchDogProc()
        {
            while (IsRunning)
            {
                try
                {
                    if (HeartRate > 0 && (DateTime.Now - this.LastSendTime).Seconds > HeartRate)
                    {
                        SendHearBeat();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger?.Error(ex, "Heart Beat");
                }

                Thread.Sleep(ReceiveTimeer);
            }
        }
        protected virtual void HandleConnected(IChannel channel)
        { 
        }
    }
}
