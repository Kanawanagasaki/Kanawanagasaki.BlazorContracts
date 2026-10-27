namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/sse/compressed-json", EVerbs.Post)]
[ContractCompression]
public class TestSseCompressedJsonContract : IContract<IAsyncEnumerable<SseItem<SimpleTestResponse>>>
{
    public int Count { get; init; }
}
