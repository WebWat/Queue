using NBomber.Configuration;
using NBomber.Contracts;
using NBomber.CSharp;
using System.Text;
using System.Text.Json;

using var httpClient = new HttpClient();

var step = Step.Create("do", timeout: TimeSpan.FromHours(1), execute: async context =>
{
    string id = Guid.NewGuid().ToString();
    int Len = 10;
    var matrix = new Matrix();
    Random random = new();

    for (int i = 0; i < Len; i++)
    {
        var row = new double[Len];

        for (int j = 0; j < Len; j++)
        {
            row[j] = random.Next(-20, 21);
        }

        matrix.Rows.Add(new Matrix.Row(row));
    }

    string json = JsonSerializer.Serialize(matrix);

    var push = await httpClient.PostAsync("https://localhost:7067/push/" + id, new StringContent(json, Encoding.UTF8, "application/json"));

    var response = await httpClient.GetStringAsync("https://localhost:7067/state/" + id);

    while (int.Parse(response) != 0)
    {
        //context.Logger.Information("Try " + i++ + " for " + id);
        response = await httpClient.GetStringAsync("https://localhost:7067/state/" + id);
        await Task.Delay(500);
    }

    var result = await httpClient.PostAsync("https://localhost:7067/Do/" + id, default);

    if (result.IsSuccessStatusCode)
        return Response.Ok();

    return Response.Fail();
});


var scenario = ScenarioBuilder.CreateScenario("test", step).WithLoadSimulations(new[] {
     Simulation.KeepConstant(3, TimeSpan.FromSeconds(60))
}); 

NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportFormats(ReportFormat.Txt, ReportFormat.Html)
    .Run();

Console.ReadKey();

public record Matrix
{
    public List<Row> Rows { get; set; } = new();

    public record Row(double[] Data)
    {
    }
}