namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestSseMsgPackHandler : IContractHandler<TestSseMsgPackContract, IAsyncEnumerable<SseItem<MessagePackWeatherForecast>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<MessagePackWeatherForecast>>>> HandleAsync(TestSseMsgPackContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<MessagePackWeatherForecast>>>(Stream(contract.Count, ct)));

    private static async IAsyncEnumerable<SseItem<MessagePackWeatherForecast>> Stream(int count, [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            yield return new SseItem<MessagePackWeatherForecast>(new MessagePackWeatherForecast
            {
                Date = new DateTime(2026, 1, 1).AddDays(i),
                TemperatureC = 10 + i,
                Summary = $"Forecast event {i}"
            }, "forecast");
        }
    }
}
