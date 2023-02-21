using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SoulFab.Core.Logger
{
    public class ConsoleLoggerWriter : ILoggerWriter
    {
        static private IDictionary<LogLevel, ConsoleColor> _EventColorMap = new Dictionary<LogLevel, ConsoleColor>()
        {
            { LogLevel.Information, ConsoleColor.White },
            { LogLevel.Warning, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
            { LogLevel.Critical, ConsoleColor.Magenta },
            { LogLevel.Trace, ConsoleColor.Cyan },
            { LogLevel.Debug, ConsoleColor.Green },
            { LogLevel.None, ConsoleColor.Gray },
        };

        public ConsoleLoggerWriter()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public void Log(LogInfo[] infos)
        {
            foreach (LogInfo li in infos)
            {
                Console.ForegroundColor = ConsoleLoggerWriter._EventColorMap[li.Type];

                if (li.Type == LogLevel.None)
                {
                    Console.WriteLine(li.Message);
                }
                else
                {
                    Console.WriteLine(li.ToString());
                }
            }
        }
    }
}
