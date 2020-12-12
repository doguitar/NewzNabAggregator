using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewzNabAggregator.Common
{
    public class TaskExecutor
    {
        Task[] _executors = null;
        uint _executorCount = 0;
        private readonly QueueChannel<Func<Task>> _taskQueue = new QueueChannel<Func<Task>>();

        public bool Stopping { get; private set; }
        public bool Stopped { get; private set; } = true;
        public bool Started { get; private set; }
        public bool Starting { get; private set; }

        public TaskExecutor(uint executorCount)
        {
            if (executorCount > 0)
            {
                _executorCount = executorCount;
            }
            else
            {
                throw new ArgumentOutOfRangeException("executorCount must be greater that zero");
            }
        }

        private async Task Executor()
        {
            while (!Stopping)
            {
                var search = await _taskQueue.Reader.ReadAsync();
                var task = search();
                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                }
                await task;
            }
            Started = false;
        }

        public void Start() { StartAsync().GetAwaiter().GetResult(); }
        public async Task StartAsync()
        {
            if (!Starting && !Started)
            {
                Starting = true;
                await StopAsync();

                _executors = Enumerable.Range(0, (int)_executorCount).Select(i => new Task(async () => await Executor())).ToArray();
                foreach (var executor in _executors)
                {
                    executor.Start();
                }
                Stopped = false;
                Started = true;
            }
        }

        public void Stop() { StopAsync().GetAwaiter().GetResult(); }
        public async Task StopAsync()
        {
            if (!Stopping && !Stopped)
            {
                Stopping = true;
                await Task.WhenAll(_executors);
                Stopped = true;
                Stopping = false;
            }
        }

        public void AddTask(Action action)
        {
            AddTaskAsync(() =>
            {
                action();
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
        }
        public async Task AddTaskAsync(Func<Task> func)
        {
            if (Stopping)
            {
                return;
            }

            await _taskQueue.Writer.WriteAsync(func);
        }

        public void AddTaskAndAwait(Action action)
        {
            AddTaskAndAwaitAsync(() =>
            {
                action();
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
        }
        public async Task AddTaskAndAwaitAsync(Func<Task> func)
        {
            if (Stopping)
            {
                return;
            }

            var semaphore = new SemaphoreSlim(0);
            await AddTaskAsync(async () =>
            {
                try
                {
                    await func();
                }
                finally
                {
                    semaphore.Release();
                }
            });
            await semaphore.WaitAsync();
        }
    }
}