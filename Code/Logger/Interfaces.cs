using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SoulFab.Core.Logger
{
    /*
        public interface ILogger
    {
        string ModuleName { get; }

        void Log(MessageType type, string msg);

        void Info(string msg);
        void Debug(string msg);
        void Warn(string msg);
        void Error(Exception e, string msg = "");
    }
    */

    [Serializable]
    public class LogInfo
    {
        static Dictionary<LogLevel, string> _TypeNames = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Information, "INF" },
            { LogLevel.Warning, "WRN" },
            { LogLevel.Error, "ERR" },
            { LogLevel.Critical, "CRT" },
            { LogLevel.Trace, "TRC" },
            { LogLevel.Debug, "DBG" },
        };

        public DateTime Time;
        public string ModuleName;
        public LogLevel Type;
        public string Message;
        public string Computer;

        public string TimeString
        {
            get 
            {
                return this.Time.ToString("HH:mm:ss.fff");
            }
        }

        public override string ToString()
        {
            string ret;

            if (this.Type == LogLevel.None)
            {
                ret = this.Message;
            }
            else 
            {
                ret = String.Format("{0:HH:mm:ss.fff} {1} [{2}] {3}", this.Time, _TypeNames[this.Type], this.ModuleName, this.Message);
            }

            return ret;
        }
    }

    public interface ILoggerManager
    {
        ILogger RegisterLogger(string module_name);

        void AppendWriter(ILoggerWriter writer);
    }

    public interface ILoggerWriter
    {
        void Log(LogInfo[] infos);
    }

    public static class LoggerExtension
    {
        static EventId DefaultEventId = new EventId(-1, "Application");

        static public void Info(this ILogger logger, string msg)
        {
            logger?.Log(LogLevel.Information, DefaultEventId, msg, null, null);
        }

        static public void Debug(this ILogger logger, string msg)
        {
            logger?.Log(LogLevel.Debug, DefaultEventId, msg, null, null);
        }

        static public void Warning(this ILogger logger, string msg)
        {
            logger?.Log(LogLevel.Warning, DefaultEventId, msg, null, null);
        }

        static public void Error(this ILogger logger, string msg)
        {
            logger?.Log(LogLevel.Error, DefaultEventId, msg, null, null);
        }

        static public void Error(this ILogger logger, Exception ex, string env = "")
        {
            string Text = String.Empty;
            Exception ie = ex.InnerException;

            if(logger != null)
            {
                string txt;

                if (String.IsNullOrEmpty(env))
                {
                    txt = ex.Message;
                }
                else
                {
                    txt = "[" + env + "] " + ex.Message;
                }
                string trace = "Source: " + ex.Source + "\nTrace: " + ex.StackTrace;

                if (ie != null)
                {
                    txt += " -> " + ie.Message;
                    trace += "\nSource: " + ie.Source + "\nTrace: " + ie.StackTrace;
                }

                logger.Log(LogLevel.Error, DefaultEventId, txt, null, null);
                logger.Log(LogLevel.Information, DefaultEventId, trace, null, null);
            }
        }

        static public void Text(this ILogger logger, string msg)
        {
            logger?.Log(LogLevel.None, DefaultEventId, msg, null, null);
        }

        static public void Text(this ILogger logger, object obj)
        {
            string msg = JsonSerializer.Serialize(obj);

            logger?.Log(LogLevel.None, DefaultEventId, msg, null, null);
        }
    }
}
