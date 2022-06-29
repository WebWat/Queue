## About this project

This **minimal web-API**, written using ASP.Net Core, is a simple example of creating a query queue for executing some "heavy" process.
In the example, the workload is the calculation of the determinant of the matrix.

The project uses **Blazor WebAssembly** as a client and [NBomber](https://github.com/PragmaticFlow/NBomber) as a load testing application.

![Blazor Index page](https://github.com/WebWat/Queue/blob/master/Images/clientPage.png)

## How the project works

### Api project

The main logic will happen on the server side (the `Api` project). Let's take a look at the `TaskQueue.cs` file. 
Initially we will consider this class as a service, which will represent the **Singleton** type. 
In the service we have a thread-safe **ConcurrentQueue**, which stores requests, and the request itself will be of type **Item** (contains a unique id and request data). 
The logic is tied with **SemaphoreSlim**, which allows to limit the number of threads that can access the resource in parallel. 
Thus, we can set the maximum number of threads which can perform a certain operation simultaneously (for the example, the limit of 1 process).

The execution process is as follows:

1.  The client transmits its unique id and data to be processed. 
    ``` csharp
    public void Push(string id, Matrix matrix)
    {
        // Handling the data
        // ...

        Queue.Enqueue(new Item { Id = id, Data = data });
    }
    ```
2.  After the client has added his request to the queue, we check the number of people in front of us. Be sure to synchronize the threads, because we use the Select extension method.
    ``` csharp
    public int GetState(string id)
    {
        lock (_locker)
        {
            return Array.IndexOf(Queue.Select(i => i.Id).ToArray(), id);
        }
    }
    ```

3.  If the previous query, returned us `0` (i.e. it means that the current query is the first in the queue). Execute it and return the result. 

    Why do we use `lock` here? Because we use in the example **SemaphoreSlim**, which means that the resource can be accessed by 1 or more threads and also that the method **First** is used.
    ``` csharp
    public async Task<double> Do(string id)
    {
        await _semaphoreSlim.WaitAsync();

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
    ```
    
It's worth paying attention to the background `CheckQueue` process. 
It's simple, there can be a situation where a client has added a request to the queue, but doesn't execute it.
This means that if the queue reaches that client and gets stuck forever. 
This service serves to remove these requests if they don't execute.
``` csharp
private async Task BackgroundProcessing(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var current = _taskQueue.GetFirstId();
        
        // If the first element in the queue is the first and the semaphore does nothing, delete it
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

        _logger.LogInformation($"Queue length: {_taskQueue.GetQueueLength()}");

        await Task.Delay(TimeSpan.FromSeconds(10));
    }
}
```

### BlazorApp1 project
