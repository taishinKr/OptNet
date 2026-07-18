using System.Collections.Concurrent;

namespace OptNet
{
    public class PCQueue : IDisposable
    {
        BlockingCollection<Task> _taskQ = new BlockingCollection<Task>();
        public PCQueue(int workerCount)
        {
            // Create and start a separate Task for each consumer
            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }
        public Task Enqueue(
                    Action action,
                    CancellationToken cancelToken = default(CancellationToken))
        {
            var task = new Task(action, cancelToken);
            _taskQ.Add(task);
            return task;
        }
        public Task<TResult> Enqueue<TResult>(
                    Func<TResult> func,
                    CancellationToken cancelToken = default(CancellationToken))
        {
            var task = new Task<TResult>(func, cancelToken);
            _taskQ.Add(task);
            return task;
        }
        void Consume()
        {
            foreach (var task in _taskQ.GetConsumingEnumerable())
                try
                {
                    if (!task.IsCanceled) task.RunSynchronously();
                }
                catch (InvalidOperationException)
                {
                }
        } // Race condition

        public void Dispose() { _taskQ.CompleteAdding(); }
    }

}
