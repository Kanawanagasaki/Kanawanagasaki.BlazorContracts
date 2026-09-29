namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class MessagePackWeatherHandler : IContractHandler<MessagePackWeatherContract, MessagePackWeatherForecast>
{
    public Task<ContractResult<MessagePackWeatherForecast>> HandleAsync(MessagePackWeatherContract contract, CancellationToken ct = default)
    {
        var rng = new Random(contract.Seed);
        var days = new MessagePackWeatherDay[Math.Max(1, contract.Days)];
        for (int i = 0; i < days.Length; i++)
        {
            days[i] = new MessagePackWeatherDay
            {
                Date = DateTime.UtcNow.AddDays(i),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            };
        }

        var forecast = new MessagePackWeatherForecast
        {
            Date = DateTime.UtcNow,
            TemperatureC = rng.Next(-20, 55),
            Summary = Summaries[rng.Next(Summaries.Length)],
            Payload = contract.Payload,
            Days = days
        };

        return Task.FromResult(new ContractResult<MessagePackWeatherForecast>(forecast));
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}
