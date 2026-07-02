namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts;

public class WeatherGetByIdHandler : IContractHandler<WeatherGetByIdContract, WeatherForecast>
{
    public Task<ContractResult<WeatherForecast>> HandleAsync(WeatherGetByIdContract contract, CancellationToken ct = default)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var all = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();

        if (contract.Id < 1 || all.Length < contract.Id)
            return Task.FromResult(new ContractResult<WeatherForecast>(StatusCodes.Status404NotFound, $"Forecast with id={contract.Id} not found."));

        return Task.FromResult(new ContractResult<WeatherForecast>(all[contract.Id - 1]));
    }
}
