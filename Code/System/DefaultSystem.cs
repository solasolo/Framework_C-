using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.System
{
    public class DefaultSystem : SystemFactory, IDisposable
    {
        public readonly bool _InConsole;
        protected int RemoteLogPort = 4444;

        public DefaultSystem(string[] args)
        {
            _InConsole = args.Contains("console");

            this.CreateWorld();
        }

        public override bool InConsole
        {
            get { return this._InConsole; }
        }

        protected override void CreateWorld()
        {
            var log = new LoggerManager();
            var logger = log.Get();

            if(this.InConsole) log.AppendWriter(new ConsoleLoggerWriter());
            log.AppendWriter(new UDPLoggerWriter(RemoteLogPort));
            log.AppendWriter(new FileLoggerWriter(RootPath + "../../Log/"));

            this.Set<ILoggerProvider>(log);
            this.Set(logger);

            logger.Info("System Started");
        }

        public ILogger Logger
        {
            get
            {
                return this.Get<ILogger>();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Logger.Info("System Shuting Down");

            var log = this.Get<ILoggerProvider>() as LoggerManager;
            if(log != null) log.Shutdown();
        }
    }
}
