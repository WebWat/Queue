using Microsoft.AspNetCore.Server.HttpSys;
using System.Threading.Tasks;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<TaskQueue>();
//builder.Services.AddSingleton<Result>();
//builder.Services.AddHostedService<QueuedHostedService>();
//builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
//{
//    return new BackgroundTaskQueue(1);
//});

//builder.Services.AddHostedService<ConsumeScopedServiceHostedService>();

var app = builder.Build();

//var monitorLoop = app.Services.GetRequiredService<MonitorLoop>();
//monitorLoop.StartMonitorLoop();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthorization();

app.MapRazorPages();

app.Run();
