using System.Collections.Concurrent;

namespace WebApplication1
{
    public class Result
    {
        public ConcurrentQueue<double[,]> result = new();
    }
    public class MonitorLoop
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        public ConcurrentQueue<Func<double[,]>> list = new();


        public MonitorLoop(IBackgroundTaskQueue taskQueue,
            ILogger<MonitorLoop> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartMonitorLoop()
        {
            _logger.LogInformation("MonitorAsync Loop is starting.");

            // Run a console user input loop in a background thread
            Task.Run(async () => await MonitorAsync());
        }

        private async ValueTask MonitorAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                if (list.TryDequeue(out Func<double[,]> result))
                {
                    // Enqueue a background work item
                    await _taskQueue.QueueBackgroundWorkItemAsync(result);
                }

                await Task.Delay(1000);
            }
        }
    }
}
