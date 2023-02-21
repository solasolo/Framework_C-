using Microsoft.Extensions.Logging;
using SoulFab.Core.Communication;
using System.Net;

namespace SoulFab.Core.Logger
{
    public class LogMessageCodec : IMessageCodec<short>
    {
        private BinaryMessageBuilder Builder = new BinaryMessageBuilder();

        public object Decode(short cmd, byte[] buf)
        {
            BinaryMessageParser parser = new BinaryMessageParser(buf);

            LogInfo Info = new LogInfo();
            Info.Type = (LogLevel)parser.GetInt();
            long ticks = parser.GetLong();
            Info.Time = DateTime.FromBinary(ticks);
            Info.ModuleName = parser.GetString();
            Info.Message = parser.GetString();

            return Info;
        }

        public byte[] Encode(short cmd, object obj)
        {
            var info = obj as LogInfo;

            Builder.Clean();
            Builder.Add((int)info.Type);
            Builder.Add(info.Time.ToBinary());
            Builder.Add(info.ModuleName);
            Builder.Add(info.Message);

            return Builder.GetData();
        }
    }

    public class UDPLogerConnection : SimpleUDPConnection
    {

        public UDPLogerConnection(int port) : base(port)
        {
        }

        public UDPLogerConnection(IPAddress host, int port) : base(host, port)
        {
        }

        protected override void HandleFrameData(IChannel channel, MessageEnity<short> msg)
        {
            
        }
    }

    public class UDPLoggerWriter : LoggerWriter
    {
        LogMessageCodec codec = new LogMessageCodec();

        private SimpleUDPConnection Server;

        public UDPLoggerWriter(int port)
        {
            this.Server = new UDPLogerConnection(port);
        }

        public override void Log(LogInfo[] infos)
        {
            foreach (var info in infos)
            {
                var data = this.codec.Encode(4, info);

                this.Server.Send(4, data);
            }
        }
    }
}