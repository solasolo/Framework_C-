using Microsoft.Extensions.Logging;
using SoulFab.Core.Logger;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.System
{
    public class ContinuousTask
    {
        static public async Task Run(string module, ILogger logger, CancellationToken cancel, Func<Task> task)
        {
            await Task.Run(async () =>
            {
                while (true)
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
