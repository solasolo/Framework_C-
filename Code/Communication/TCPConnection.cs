using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;

namespace SoulFab.Core.Communication
{
    public abstract class TCPConnection<T> : BaseFrameChannel<T>
    {
        protected Socket InnerSocket;
        protected IPEndPoint LocalEndPoint;
        protected IPEndPoint RemoteEndPoint;

        protected override void Release()
        {
            if (InnerSocket != null)
            {
                try
                {
                    if (InnerSocket.Connected)
                    {
                        InnerSocket.Shutdown(SocketShutdown.Both);
                        InnerSocket.Close();
                    }
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

        public override void Send(byte[] data)
        {
            try
            {
                if (InnerSocket != null && _IsOnline)
                {
                    InnerSocket.Send(data, 0, data.Length, SocketFlags.None);
                }
                else
                {
                    //
                }

                this.LastSendTime = DateTime.Now;
            }
            catch (SocketException ex)
            {
                _IsOnline = false;

                this.Logger?.Error(ex, "TCP Send");
            }
            catch (Exception ex)
            {
                this.Logger?.Error(ex);
            }
        }

        protected override int Receive(ref byte[] buf)
        {
            int ret;

            ret = this.InnerSocket.Available;
            if (ret > 0)
            {
                buf = new byte[ret];
                ret = this.InnerSocket.Receive(buf);
            }

            return ret;
        }
    }
}
