using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Logger
{
    internal class TextUDPLoggerWriter
    {
        static byte[] RTLF;

        UdpClient Sender;
        IPEndPoint Address;
        Stream Buffer;
        byte[] TypeBuffer;

        public TextUDPLoggerWriter(string host, int port)
        {
            if (TextUDPLoggerWriter.RTLF == null)
            {
                TextUDPLoggerWriter.RTLF = new byte[2] { 0x0a, 0x0d };
            }

            this.Buffer = new MemoryStream();
            this.TypeBuffer = new byte[1];

            IPHostEntry iphost = Dns.GetHostEntry(host);
            foreach (IPAddress addr in iphost.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    this.Address = new IPEndPoint(addr, port);
                }
            }

            this.Sender = new UdpClient();
            this.Sender.Connect(this.Address);
        }

        public void Log(LogInfo[] infos)
        {
            byte[] stream = null;

            foreach (LogInfo info in infos)
            {
                this.TypeBuffer[0] = (byte)info.Type;
                Buffer.Write(this.TypeBuffer, 0, 1);

                stream = Encoding.UTF8.GetBytes(info.ToString());
                Buffer.Write(stream, 0, stream.Length);
                Buffer.Write(TextUDPLoggerWriter.RTLF, 0, 2);
            }

            stream = new byte[Buffer.Length];
            Buffer.Seek(0, SeekOrigin.Begin);
            Buffer.Read(stream, 0, (int)Buffer.Length);
            Buffer.SetLength(0);
            this.Sender.Send(stream, stream.Length);
        }
    }
}
