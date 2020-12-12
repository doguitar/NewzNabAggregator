using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NewzNabAggregator.Common
{

    public class QueueChannel<T> : Channel<T>
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private SemaphoreSlim _lock = new SemaphoreSlim(0);

        public class QueueReader<R> : ChannelReader<R>
        {
            QueueChannel<R> Channel { get; }

            public QueueReader(QueueChannel<R> channel)
            {
                Channel = channel;
            }
            public override bool TryRead(out R item)
            {
                Channel._lock.Wait();
                return Channel._queue.TryDequeue(out item);
            }

            public override async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
            {
                await Channel._lock.WaitAsync(cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                {
                    Channel._lock.Release();
                    return true;
                }
                return false;
            }
        }
        public class QueueWriter<W> : ChannelWriter<W>
        {
            QueueChannel<W> Channel { get; }

            public QueueWriter(QueueChannel<W> channel)
            {
                Channel = channel;
            }

            public override bool TryWrite(W item)
            {
                Channel._queue.Enqueue(item);
                Channel._lock.Release();
                return true;
            }

            public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default)
            {
                return new ValueTask<bool>(true);
            }
        }
        public QueueChannel()
        {
            Reader = new QueueReader<T>(this);
            Writer = new QueueWriter<T>(this);
        }
    }
}