using Microsoft.Extensions.Logging;
using SoulFab.Core.Config;
using SoulFab.Core.Data;
using SoulFab.Core.System;
using System;

namespace SoulFab.Core.Service
{
    public abstract class DefaultSystemFactory : SystemFactory
    {
        public IConfig Config
        {
            get
            {
                return this.Get<IConfig>();
            }
        }

        public ILogger Logger
        {
            get
            {
                return this.Get<ILogger>();
            }
        }
    }

    public class ServiceApplication
    {
        static public void CreateWorld(SystemFactory factory, Action<SystemFactory> proc)
        {
            proc(factory);
        }
    }
}
