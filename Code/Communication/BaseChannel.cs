using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public abstract class BaseChannel : IChannel 
    {
        protected ILogger Logger;

        public void setLogger(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract void Send(byte[] data);

        protected abstract void HandleData(IChannel channel, byte[] data); 
    }

    public abstract class BaseAsyncChannel : IAsyncChannel
    {
        protected ILogger Logger;

        public void setLogger(ILogger logger)
        {
            this.Logger = logger;
        }

        public abstract Task Send(byte[] data);

        protected abstract void HandleData(IAsyncChannel channel, byte[] data);
    }
}
