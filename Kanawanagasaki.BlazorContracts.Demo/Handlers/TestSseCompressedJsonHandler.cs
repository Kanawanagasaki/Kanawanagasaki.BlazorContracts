namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestSseCompressedJsonHandler : IContractHandler<TestSseCompressedJsonContract, IAsyncEnumerable<SseItem<SimpleTestResponse>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>> HandleAsync(TestSseCompressedJsonContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<SimpleTestResponse>>>(Stream(contract.Count, ct)));

    private static async IAsyncEnumerable<SseItem<SimpleTestResponse>> Stream(int count, [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (var i = 0; i < count; i++)
        {
            ct.ThrowIfCancellationRequested();
            yield return new SseItem<SimpleTestResponse>(new SimpleTestResponse { Message = $"Compressed event {i}" }, "tick");
        }
    }
}
