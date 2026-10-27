namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestSseStreamingHandler : IContractHandler<TestSseStreamingContract, IAsyncEnumerable<SseItem<SimpleTestResponse>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>> HandleAsync(TestSseStreamingContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>(Stream(contract.Count, ct)));

    private static async IAsyncEnumerable<SseItem<SimpleTestResponse>> Stream(int count, [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(250, ct);
            yield return new SseItem<SimpleTestResponse>(new SimpleTestResponse { Message = $"Streamed event {i}" }, "tick");
        }
    }
}
