using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public class SimpleTCPServer : TCPServer<short>
    {
        protected IMessageCodec<short> MessageCodec;

        public SimpleTCPServer(string host, int port)
            : base(host, port)
        {
            var codec = new SimpleFrameCodec();
            this.SetFrameCodec(codec);

            this.MessageCodec = new SimpleMessageCodec();
        }

        protected override void HandleConnected(IChannel channel)
        {
        }

        protected override void HandleFrameData(IChannel channel, MessageEnity<short> msg)
        {
            this.MessageCodec.Decode(msg.Code, msg.Data);
        }
    }

    public class SimpleTCPClient : TCPClient<short>
    {
        private IChannelCallback<short> Callback = null;
        protected IMessageCodec<short> MessageCodec;

        public SimpleTCPClient(string host, int port)
            : base(host, port)
        {
            var codec = new SimpleFrameCodec();
            this.SetFrameCodec(codec);

            this.MessageCodec = new SimpleMessageCodec();
        }

        public void setCallbace(IChannelCallback<short> callback)
        {
            this.Callback = callback;
        }

        protected override void HandleFrameData(IChannel channel, MessageEnity<short> msg)
        {
            this.MessageCodec.Decode(msg.Code, msg.Data);
        }
    }
}
