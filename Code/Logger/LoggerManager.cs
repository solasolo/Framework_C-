using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;

namespace SoulFab.Core.Logger
{
    public abstract class LoggerWriter : ILoggerWriter
    {
        #region ILoggerWriter Members

        public abstract void Log(LogInfo[] infos);

        #endregion
    }

    public abstract class BaseLogger : ILogger
    {
        protected string ComputerName;

        public BaseLogger()
        {
            this.ComputerName = Dns.GetHostName();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public abstract void Log<TState>(LogLevel type, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);

    }

    internal class InternalLogger : BaseLogger
    {
        private string _ModuleName = String.Empty;
        protected ILoggerManager Manager;

        public InternalLogger(ILoggerManager man, string module)
            : base()
        {
            this.Manager = man;
            this._ModuleName = module;
        }

        #region ILogger Members

        public string ModuleName
        {
            get
            {
                return _ModuleName;
            }
        }

        public override void Log<TState>(LogLevel type, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogInfo li = new LogInfo();

            li.Time = DateTime.Now;
            li.Type = type;
            li.Message = state.ToString();
            li.ModuleName = this.ModuleName;
            li.Computer = this.ComputerName;

            (Manager as LoggerManager).WriteLog(li);
        }

        #endregion
    }


    public class LoggerManager : ILoggerManager, ILoggerProvider, IDisposable
    {
        private bool Disposed;
        private Thread LogThread;

        private ManualResetEvent LogEvent;
        private ManualResetEvent QuittingEvent;
        private Queue<LogInfo> WorkingQueue;
        private Queue<LogInfo> WaitingQueue;

        private List<ILoggerWriter> Writers;
        private IDictionary<string, ILogger> ModuleLoggers;
        private ILoggerWriter BackupWriter;

        static private ILoggerManager _Instance = null;

        public ILogger Get(string module = "")
        {
            if (String.IsNullOrEmpty(module))
            {
                module = AppDomain.CurrentDomain.FriendlyName;
            }


            return this.RegisterLogger(module);
        }

        static public LoggerManager operator +(LoggerManager me, ILoggerWriter writer)
        {
            me.AppendWriter(writer);

            return me;
        }

        public LoggerManager()
        {
            this.LogThread = new Thread(new ThreadStart(LogThreadProc));
            this.LogEvent = new ManualResetEvent(false);
            this.QuittingEvent = new ManualResetEvent(false);

            this.WorkingQueue = new Queue<LogInfo>();
            this.WaitingQueue = new Queue<LogInfo>();

            this.Writers = new List<ILoggerWriter>();
            this.ModuleLoggers = new Dictionary<string, ILogger>();
            this.BackupWriter = null;

            LogThread.Start();
        }

        public LoggerManager(ILoggerWriter backup)
            : this()
        {
            this.BackupWriter = backup;
        }

        ~LoggerManager()
        {
            this.Shutdown();
        }

        public virtual void Shutdown()
        {
            this.QuittingEvent.Set();
        }

        internal void WriteLog(LogInfo info)
        {
            this.DoLog(info);
        }

        #region ILoggerManager Members

        public ILogger RegisterLogger(string module)
        {
            return GetLoggerByModule(module);
        }

        public void AppendWriter(ILoggerWriter writer)
        {
            this.Writers.Add(writer);
        }

        #endregion

        private void LogThreadProc()
        {
            while (!QuittingEvent.WaitOne(100, false))
            {
                try
                {
                    if (LogEvent.WaitOne(1000, false))
                    {
                        BatchLog();
                        LogEvent.Reset();
                    }
                    else
                    {
                        DoLog(null);
                    }
                }
                catch
                {

                }
            }
        }

        private ILogger GetLoggerByModule(string module)
        {
            ILogger Result = null;

            if (this.ModuleLoggers.ContainsKey(module))
            {
                Result = this.ModuleLoggers[module];
            }
            else
            {
                Result = this.CreateLogger(module);
                this.ModuleLoggers[module] = Result;
            }

            return Result;
        }

        protected virtual void DoLog(LogInfo info)
        {
            try
            {
                lock (WaitingQueue)
                {
                    if (info != null)
                    {
                        WaitingQueue.Enqueue(info);
                    }

                    if (!LogEvent.WaitOne(0, false))
                    {
                        if (WorkingQueue.Count == 0)
                        {
                            Queue<LogInfo> Temp;

                            Temp = WaitingQueue;
                            WaitingQueue = WorkingQueue;
                            WorkingQueue = Temp;
                        }

                        LogEvent.Set();
                    }
                }
            }
            catch
            {
                //TODO: Do Nothing
            }
        }

        protected virtual void BatchLog()
        {
            if (WorkingQueue.Count > 0)
            {
                LogInfo[] Infos = WorkingQueue.ToArray();
                WorkingQueue.Clear();

                foreach (ILoggerWriter writer in Writers)
                {
                    try
                    {
                        writer.Log(Infos);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            LogInfo ErrorInfo = new LogInfo();

                            ErrorInfo.Type = LogLevel.Error;
                            ErrorInfo.Message = "Error in Write Log: " + ex.Message;
                            ErrorInfo.Time = DateTime.Now;
                            ErrorInfo.ModuleName = Process.GetCurrentProcess().MainModule.ModuleName;

                            BackupWriter?.Log(new LogInfo[1] { ErrorInfo });
                        }
                        catch
                        {
                            //
                        }
                    }
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Shutdown();
        }

        #endregion

        #region ILoggerProvider Members

        public ILogger CreateLogger(string module)
        {
            return new InternalLogger(this, module);
        }

        #endregion
    }
}
