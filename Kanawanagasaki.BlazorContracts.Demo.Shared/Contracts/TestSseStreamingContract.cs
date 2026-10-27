namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/sse/streaming", EVerbs.Get)]
public class TestSseStreamingContract : IContract<IAsyncEnumerable<SseItem<SimpleTestResponse>>>
{
    public int Count { get; init; }
}
