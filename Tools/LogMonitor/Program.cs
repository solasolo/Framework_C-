using SoulFab.Core.Logger;
using SoulFab.Core.Communication;
using System.Net;
using Microsoft.Extensions.Logging;

var monitor = new Monitor();
monitor.setLogger(new ConsoleLogger());
monitor.Startup();

while (true)
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.Escape)
        {
            break;
        }
    }

    Thread.Sleep(100);
}

monitor.Shutdown();

class ConsoleLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(formatter(state, exception));
    }
}

class Monitor : UDPLogerConnection
{
    private ILoggerWriter Logger;

    public Monitor() : base(IPAddress.Any, 4444)
    {
        this.Logger = new ConsoleLoggerWriter();

        this.setMessageCodec(new LogMessageCodec());
    }

    protected override void HandleFrameData(IChannel channel, MessageEnity<short> msg)
    {
        if (msg.Code == 4)
        {
            var Info = this.MessageCodec.Decode(msg.Code, msg.Data);

            this.Logger.Log(new LogInfo[] { Info as LogInfo });
        }
    }
}