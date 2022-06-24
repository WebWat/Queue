// See https://aka.ms/new-console-template for more information


Console.WriteLine("Press smth to start:");
Console.ReadKey();
Console.WriteLine("Starting...");

using var client = new HttpClient();
int i = 0;

//Task task1 = new Task(() => Operation(100));
//Task task2 = new Task(() => Operation(1000));

//task1.Start();
//task2.Start();
//Task.WaitAny(task1, task2);

Parallel.Invoke(() => Operation(100), () => Operation(1000));
//var client2 = Task.Run(async () =>
//{
//    await Operation("Client_2");
//});

Console.ReadLine();


static void Operation(object obj)
{
    using var client = new HttpClient();
    int i = (int)obj;

    while (i < 5000)
    {
        Console.WriteLine($"\n=========== {Thread.CurrentThread.ManagedThreadId} ===========\nTry request {i + 1}");
        var response = client.GetStringAsync("https://localhost:7067/state/" + i).Result;

        if (int.Parse(response) == 0)
        {
            Console.WriteLine("Proccesing...");
            response = client.GetStringAsync("https://localhost:7067/Do/" + i).Result;
            Console.WriteLine(response);
            Console.WriteLine("Success for request " + i + 1);
            i++;
        }
        else
        {
            Console.WriteLine("Queue count: " + int.Parse(response));
        }

        Thread.Sleep(500);
    }
}