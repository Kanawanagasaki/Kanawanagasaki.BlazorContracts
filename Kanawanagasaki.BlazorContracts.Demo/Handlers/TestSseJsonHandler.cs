namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestSseJsonHandler : IContractHandler<TestSseJsonContract, IAsyncEnumerable<SseItem<SimpleTestResponse>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>> HandleAsync(TestSseJsonContract contract, CancellationToken ct = default)
    {
        if (contract.Count < 0)
            return Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>(400, "Count must be non-negative."));

        return Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>(Stream(contract.Count, ct)));
    }

    private static async IAsyncEnumerable<SseItem<SimpleTestResponse>> Stream(int count, [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            yield return new SseItem<SimpleTestResponse>(new SimpleTestResponse { Message = $"Event {i}" }, "tick")
            {
                EventId = i.ToString(),
                ReconnectionInterval = i == 0 ? TimeSpan.FromMilliseconds(1000) : null,
            };
        }
    }
}
