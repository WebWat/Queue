## About this project

This **minimal web-API**, written using ASP.Net Core, is a simple example of creating a query queue for executing some "heavy" process.
In the example, the workload is the calculation of the determinant of the matrix.

The project uses **Blazor WebAssembly** as a client and [NBomber](https://github.com/PragmaticFlow/NBomber) as a load testing application.

![Blazor Index page](https://github.com/WebWat/Queue/blob/master/Images/clientPage.png)

## How the project works

The main logic will happen on the server side (the `Api` project). Let's take a look at the `TaskQueue.cs` file. 
Initially we will consider this class as a service, which will represent the **Singleton** type. 
In the service we have a thread-safe **ConcurrentQueueue**, which stores requests, and the request itself will be of type **Item** (contains a unique id and request data). 
The logic is tied with **SemaphoreSlim**, which allows to limit the number of threads that can access the resource in parallel. 
Thus, we can set the maximum number of threads which can perform a certain operation simultaneously (for the example, the limit of 1 process).

The execution process is as follows:
* The client transmits its unique id and data to be processed. 
  ``` csharp
  public void Push(string id, Matrix matrix)
  {
      // Handling the data
      // ...

      Queue.Enqueue(new Item { Id = id, Data = data });
  }
  ```


Translated with www.DeepL.com/Translator (free version)

