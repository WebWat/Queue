using System.Collections.Concurrent;

namespace Api
{
    // TODO: add logging
    // check memory
    public class TaskQueue
    {
        private class Item
        {
            public string Id;
            public double[,] Data;
        }

        private readonly object _locker = new object();
        private ConcurrentQueue<Item> Queue = new();
        private ILogger<TaskQueue> _logger;
        private readonly SemaphoreSlim _semaphoreSlim;
        public readonly int MaxThreads;

        public int GetCurrentCount()
        {
            return _semaphoreSlim.CurrentCount;
        }

        public string GetFirstId()
        {
            if (Queue.Count != 0)
                return Queue.First().Id;

            return null;
        }

        public TaskQueue(ILogger<TaskQueue> logger)
        {
            MaxThreads = Environment.ProcessorCount / 2;

            _logger = logger;
            _semaphoreSlim = new SemaphoreSlim(MaxThreads, MaxThreads);
        }

        public void Dequeue()
        {
            Queue.TryDequeue(out _);
        }

        public int GetState(string id, double[,] data)
        {
            lock (_locker)
            {
                var index = Array.IndexOf(Queue.Select(i => i.Id).ToArray(), id);

                if (index == -1)
                {
                    Queue.Enqueue(new Item { Id = id, Data = data });
                    return Queue.Count - 1;
                }
                else
                    return index;
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
                            return Determinant(temp.Data);
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

        public double Determinant(double[,] _array)
        {
            if (_array.GetLength(0) != _array.GetLength(1))
                throw new ArgumentException("The matrix must be square");

            switch (_array.GetLength(0))
            {
                case 1:
                    return _array[0, 0];
                case 2:
                    return _array[0, 0] * _array[1, 1] - _array[0, 1] * _array[1, 0];
                case 3:
                    return _array[0, 0] * _array[1, 1] * _array[2, 2] +
                           _array[0, 1] * _array[1, 2] * _array[2, 0] +
                           _array[0, 2] * _array[1, 0] * _array[2, 1] -
                           _array[0, 2] * _array[1, 1] * _array[2, 0] -
                           _array[0, 0] * _array[1, 2] * _array[2, 1] -
                           _array[0, 1] * _array[1, 0] * _array[2, 2];
                default:
                    double result = 0;

                    for (int i = 0; i < _array.GetLength(1); i++)
                    {
                        double value = _array[0, i];
                        var matrix = new double[_array.GetLength(1) - 1, _array.GetLength(1) - 1];
                        int column = 0;

                        for (int a = 1; a < _array.GetLength(0); a++)
                        {
                            for (int b = 0; b < _array.GetLength(1); b++)
                            {
                                if (b != i)
                                {
                                    matrix[a - 1, column] = _array[a, b];
                                    column++;
                                }
                            }
                            column = 0;
                        }

                        result += (i % 2 == 0 ? 1 : (-1)) * value * Determinant(matrix);
                    }

                    return result;
            }
        }
    }
}
