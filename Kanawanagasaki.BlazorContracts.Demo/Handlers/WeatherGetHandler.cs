namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts;

public class WeatherGetHandler : IContractHandler<WeatherGetContract, WeatherForecast[]>
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public Task<ContractResult<WeatherForecast[]>> HandleAsync(WeatherGetContract contract, CancellationToken ct = default)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = contract.WithSummary ? Summaries[Random.Shared.Next(Summaries.Length)] : null,
            City = contract.City
        }).ToArray();

        return Task.FromResult(new ContractResult<WeatherForecast[]>(forecasts));
    }
}
