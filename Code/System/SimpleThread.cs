using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SoulFab.Core.System
{
    public abstract class SimpleThread
    {
        private bool IsRunning;
        private Thread TheThread;

        protected ILogger Logger;
        protected int SleepTime;

        protected abstract void DoTask();

        public SimpleThread(ILogger logger)
        {
            this.IsRunning = false;
            this.SleepTime = 10;
            this.Logger = logger;

            this.TheThread = new Thread(this.ThreadProc);
        }

        public void Start()
        {
            IsRunning = true;
            this.TheThread.Start(this);
        }

        public virtual void Shutdown()
        {
            this.IsRunning = false;
        }

        private void ThreadProc(object obj)
        {
            SimpleThread me = obj as SimpleThread;

            while (this.IsRunning)
            {
                try
                {
                    this.DoTask();
                }
                catch (Exception ex)
                {
                    if (this.Logger != null)
                    {
                        this.Logger.Error(ex);
                    }
                }

                Thread.Sleep(this.SleepTime);
            }
        }

    }
}
