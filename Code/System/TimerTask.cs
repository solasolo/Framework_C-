using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.System
{
    public class TimerTask
    {
        static public async Task Run(string module, ILogger logger, CancellationToken cancel, int interval, Func<Task> task)
        {
            await Task.Run(async () =>
            {
                var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));
                while (await timer.WaitForNextTickAsync(cancel))
                {
                    try
                    {
                        await task().WaitAsync(cancel);
                    }
                    catch (OperationCanceledException ex)
                    {
                        logger.Info($"[{module}] Exit");
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, module);
                    }

                    if (cancel.IsCancellationRequested) break;
                }
            });
        }
    }
}
