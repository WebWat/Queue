using System.Collections.Concurrent;

namespace WebApplication1
{
    public class Test
    {
        public string id;
        public Task<double[,]> task;
    }

    public class TaskQueue
    {
        readonly object _locker = new object();
        public Queue<Test> result = new();
        //public ConcurrentQueue<Test> temp = new();
        private ILogger<TaskQueue> _logger;
        private readonly SemaphoreSlim _semaphoreSlim;

        public TaskQueue(ILogger<TaskQueue> logger)
        {
            _logger = logger;
            _semaphoreSlim = new SemaphoreSlim(1, 1);
        }

        private async Task Add(string id)
        {
            
        }
    }
}