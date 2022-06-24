using Api;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddHostedService<CheckQueue>();
builder.Services.AddCors();
builder.Services.AddSingleton<TaskQueue>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyOrigin());

app.MapGet("/state/{id}", (string id, TaskQueue queue) =>
{
    var data = GenerateMatrix(11);
    return queue.GetState(id, data);
});

app.MapGet("/Do/{id}", async (string id, TaskQueue queue) =>
{
    var result = await queue.Do(id);

    if (result is 0)
        return Results.BadRequest();
    return Results.Text(result.ToString());
});

app.Run();
//var data = new HttpClient();
//var rt = await data.GetAsync("");
//rt.StatusCode.ToString();


static double[,] GenerateMatrix(int row)
{
    Random random = new Random();

    var result = new double[row, row];

    for (int i = 0; i < row; i++)
    {
        for (int j = 0; j < row; j++)
        {
            result[i, j] = random.Next(-20, 21) + random.NextDouble();
        }
    }

    return result;
}


static string Convert(double[,] data)
{
    StringBuilder builder = new StringBuilder();

    for (int i = 0; i < data.GetLength(0); i++)
    {
        for (int j = 0; j < data.GetLength(1); j++)
        {
            builder.Append($"{data[i, j]:f2}\t");
        }

        builder.AppendLine();
    }

    return builder.ToString();
}