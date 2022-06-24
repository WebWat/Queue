namespace Api
{
    public class CheckQueue : BackgroundService
    {
        private readonly ILogger<CheckQueue> _logger;
        private readonly TaskQueue _taskQueue;
        private string _temp = null;

        public CheckQueue(TaskQueue taskQueue,
            ILogger<CheckQueue> logger)
        {
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting...");
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var current = _taskQueue.GetFirstId();

                if (_temp != null && current == _temp && _taskQueue.GetCurrentCount() == _taskQueue.MaxThreads)
                {
                    _logger.LogInformation($"Remove {current}");
                    _taskQueue.Dequeue();
                    _temp = null;
                }
                else
                {
                    _temp = current;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
