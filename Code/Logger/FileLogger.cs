using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SoulFab.Core.Logger
{
    public class FileLoggerWriter : LoggerWriter
    {
        private string BasePath = String.Empty;

        public FileLoggerWriter(string path)
        {
            BasePath = path;
        }

        public override void Log(LogInfo[] infos)
        {
            foreach (LogInfo li in infos)
            {
                StreamWriter sr = new StreamWriter(this.BasePath + li.ModuleName + li.Time.ToString(" yyyy-MM-dd") + ".log", true);

                if (li.Type == LogLevel.None)
                {
                    sr.WriteLine(li.Message);
                }
                else 
                {
                    sr.WriteLine(li.ToString());
                }

                sr.Flush();
                sr.Close();
            }
        }
    }

    public class FileLogger : BaseLogger
    {
        private FileLoggerWriter Writer;
        private string ModuleName;

        public FileLogger(string path, string module)
            : base()
        {
            this.Writer = new FileLoggerWriter(path);
            this.ModuleName = module;
        }

        public override void Log<TState>(LogLevel type, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogInfo li = new LogInfo();

            li.Time = DateTime.Now;
            li.Type = type;
            li.Message = state.ToString();
            li.ModuleName = this.ModuleName;
            li.Computer = this.ComputerName;

            this.Writer.Log(new LogInfo[] { li });
        }
    }
}
