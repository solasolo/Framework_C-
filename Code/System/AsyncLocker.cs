using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulFab.Core.System
{
    public sealed class AsyncSemaphore
    {
        private SemaphoreSlim _Semaphore;

        private class SemaphoreReleaser : IDisposable
        {
            private SemaphoreSlim _Semaphore;

            public SemaphoreReleaser(SemaphoreSlim semaphore)
            {
                this._Semaphore = semaphore;
            }

            public void Dispose()
            {
                _Semaphore.Release();
            }
        }

        public AsyncSemaphore()
        {
            this._Semaphore = new SemaphoreSlim(1);
        }

        public async Task<IDisposable> WaitAsync()
        {
            await this._Semaphore.WaitAsync();

            return new SemaphoreReleaser(this._Semaphore);
        }
    }
}
