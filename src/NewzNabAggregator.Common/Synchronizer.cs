using System;
using System.Threading;
using System.Threading.Tasks;

namespace NewzNabAggregator.Common
{
    public class Synchronizer<T> : IDisposable where T : IDisposable
    {
        readonly TaskExecutor _executor;
        bool _disposed;
        T _context;

        public Synchronizer(T context)
        {
            _context = context;
            _executor = new TaskExecutor(1);
            _executor.Start();
        }
        ~Synchronizer()
        {
            Dispose(false);
        }

        public async Task<R> SynchronizeAsync<R>(Func<T, Task<R>> action)
        {
            var semaphore = new SemaphoreSlim(0);
            R result = default;
            await _executor.AddTaskAsync(async () =>
            {
                try
                {
                    result = await action(_context);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await semaphore.WaitAsync();
            return result;
        }
        public async Task SynchronizeAsync(Func<T, Task> action)
        {
            var semaphore = new SemaphoreSlim(0);
            await _executor.AddTaskAsync(async () =>
            {
                try
                {
                    await action(_context);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await semaphore.WaitAsync();
        }

        public void Synchronize(Action<T> action)
        {
            var semaphore = new SemaphoreSlim(0);
            _executor.AddTask(() =>
            {
                try
                {
                    action(_context);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            semaphore.Wait();
            return;
        }
        public R Synchronize<R>(Func<T, R> action)
        {
            var semaphore = new SemaphoreSlim(0);
            R result = default;
            _executor.AddTask(() =>
            {
                try
                {
                    result = action(_context);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            semaphore.Wait();
            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            };

            if (disposing)
            {
                _executor.AddTaskAndAwait(() =>
                {
                    _executor.Stop();
                    _context.Dispose();
                });
            }

            _disposed = true;
        }
    }
}
