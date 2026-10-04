namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class MessagePackCompressHandler : IContractHandler<MessagePackCompressContract, MessagePackWeatherForecast>
{
    public Task<ContractResult<MessagePackWeatherForecast>> HandleAsync(MessagePackCompressContract contract, CancellationToken ct = default)
    {
        var forecast = new MessagePackWeatherForecast
        {
            Date = DateTime.UtcNow,
            TemperatureC = contract.Seed,
            Summary = contract.City
        };

        return Task.FromResult(new ContractResult<MessagePackWeatherForecast>(forecast));
    }
}
