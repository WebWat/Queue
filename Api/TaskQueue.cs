using System.Collections.Concurrent;

namespace Api
{
    public class TaskQueue
    {
        private class Item
        {
            public string Id;
            public double[,] Data;
        }

        private readonly object _locker = new object();
        private ConcurrentQueue<Item> Queue = new();
        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly ILogger<TaskQueue> _logger;
        public readonly int MaxThreads;

        public int GetCurrentCount()
        {
            return _semaphoreSlim.CurrentCount;
        }

        public int GetQueueLength()
        {
            return Queue.Count;
        }

        public string GetFirstId()
        {
            if (Queue.Count != 0)
                return Queue.First().Id;

            return null;
        }

        public TaskQueue(ILogger<TaskQueue> logger)
        {
            MaxThreads = 1;

            _semaphoreSlim = new SemaphoreSlim(MaxThreads, MaxThreads);
            _logger = logger;
        }

        public void Dequeue()
        {
            Queue.TryDequeue(out _);
        }

        public void Push(string id, Matrix matrix)
        {
            var data = new double[matrix.Rows.Count, matrix.Rows.Count];

            for (int i = 0; i < matrix.Rows.Count; i++)
            {
                for (int j = 0; j < matrix.Rows.Count; j++)
                {
                    data[i, j] = matrix.Rows[i].Data[j];
                }
            }

            Queue.Enqueue(new Item { Id = id, Data = data });
        }


        public int GetState(string id)
        {
            lock (_locker)
            {
                return Array.IndexOf(Queue.Select(i => i.Id).ToArray(), id);
            }
        }

        public async Task<double> Do(string id)
        {
            await _semaphoreSlim.WaitAsync();

            Task<double> resultTask = null;
            Item temp = new();
            bool execute = false;

            try
            {
                lock (_locker)
                {
                    if (Queue.Count == 0)
                        return 0;

                    temp = Queue.First();

                    if (temp.Id == id)
                    {
                        resultTask = new Task<double>(() =>
                        {
                            return Operation.Determinant(temp.Data);
                        });

                        Queue.TryDequeue(out _);
                    }
                    else
                    {
                        return 0;
                    }
                }

                _logger.LogInformation("Execute task " + temp.Id);
                execute = true;

                resultTask.Start();
                return await resultTask;
            }
            finally
            {
                _semaphoreSlim.Release();

                if (execute)
                    _logger.LogInformation("Completed task " + temp.Id);
            }
        }       
    }
}
