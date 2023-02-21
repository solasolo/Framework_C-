using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    static public class BaseSocket
    {
        static public IPEndPoint MakeEndPoint(string host, int port)
        {
            IPEndPoint Result = null;
            IPAddress ip;

            if (!IPAddress.TryParse(host, out ip))
            {
                IPHostEntry iphost = Dns.GetHostEntry(host);
                foreach (IPAddress addr in iphost.AddressList)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ip = addr;
                        break;
                    }
                }
            }

            if (ip != null)
            {
                Result = new IPEndPoint(ip, port);
            }
            else
            {
                throw new Exception("无法定位服务器地址");
            }

            return Result;
        }
    }
}
