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

        public TaskQueue()
        {
            MaxThreads = 1;

            _semaphoreSlim = new SemaphoreSlim(MaxThreads, MaxThreads);
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
            Item temp;

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

                resultTask.Start();
                return await resultTask;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }       
    }
}
