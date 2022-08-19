using Api;
using Microsoft.OpenApi.Models;
using System.Net.Mime;
using Api.Logger;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1.0", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Queue API",
        Description = "An ASP.NET Core Web API for creating a query queue",
    });
});

builder.Services.AddLogging();
builder.Services.AddHostedService<CheckQueue>();
builder.Services.AddCors();
builder.Services.AddSingleton<TaskQueue>();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddFile("app.log", append: true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Queue API v1.0"));
}

app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader());

app.MapPost("/push/{id}", (string id, Matrix matrix, TaskQueue queue) =>
{
    queue.Push(id, matrix);
})
    .Produces(StatusCodes.Status200OK)
    .WithName("Push")
    .WithTags("Queue");

app.MapGet("/state/{id}", (string id, TaskQueue queue) =>
{
    return queue.GetState(id);
})
    .Produces<int>(StatusCodes.Status200OK)
    .WithName("GetState")
    .WithTags("Queue");

app.MapPost("/do/{id}", async (string id, TaskQueue queue) =>
{
    var result = await queue.Do(id);

    if (result is 0)
        return Results.BadRequest();

    return Results.Text(result.ToString());
})
    .Produces(StatusCodes.Status200OK, contentType: MediaTypeNames.Text.Plain)
    .Produces(StatusCodes.Status400BadRequest)
    .WithName("Process")
    .WithTags("Queue");

app.Run();

public record Matrix
{
    public List<Row> Rows { get; set; } = new();

    public record Row(double[] Data)
    {
    }
}