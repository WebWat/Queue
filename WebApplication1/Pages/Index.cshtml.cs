using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Concurrent;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ConcurrentQueue<Task<double[,]>> list = new();
        private object _locker = new object();
        public int Count = 0;
        public double[,] test;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync([FromServices] TaskQueue queue)
        {
            string id = HttpContext.Connection.Id;

            Test item = null;

            lock (_locker)
            {
                queue.result.Enqueue(new Test
                {
                    task = new Task<double[,]>(() =>
                    {
                        Task.Delay(500).Wait();
                        return GenerateMatrix(5, 5);
                    }),
                    id = id
                });

                _logger.LogInformation("Create for " + id);

                var temp = queue.result.First();

                if (temp != null && temp.id == id)
                {
                    queue.result.TryDequeue(out item);
                }
            }

            if (item != null)
            {
                item.task.Start();
                test = await item.task;

                _logger.LogInformation("Done! For connection " + id + " Count: " + queue.result);
            }
            else
            {
                Count = queue.result.Count;
                _logger.LogWarning(Count.ToString());
            }

            return Page();
        }

        private double[,] Multiply(double[,] a, double[,] b)
        {
            if (a.GetLength(1) != b.GetLength(0))
                throw new ArgumentException("The columns of the first matrix " +
                    "must be equal to the rows of the second matrix");

            var result = new double[a.GetLength(0), b.GetLength(1)];

            for (int i = 0; i < a.GetLength(0); i++)
            {
                int row = 0;

                while (row < b.GetLength(1))
                {
                    for (int j = 0; j < a.GetLength(1); j++)
                    {
                        result[i, row] += a[i, j] * b[j, row];
                    }

                    row++;
                }

            }

            return result;
        }

        private double[,] GenerateMatrix(int row, int column)
        {
            Random random = new Random();

            var result = new double[row, column];

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    result[i, j] = random.Next(-20, 21) + random.NextDouble();
                }
            }

            return result;
        }
    }
}