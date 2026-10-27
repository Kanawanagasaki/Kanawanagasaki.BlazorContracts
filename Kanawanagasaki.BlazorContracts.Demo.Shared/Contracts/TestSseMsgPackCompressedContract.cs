namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/test/sse/msgpack-compressed", EVerbs.Post)]
[MessagePackObject]
[ContractCompression]
public class TestSseMsgPackCompressedContract : IContract<IAsyncEnumerable<SseItem<MessagePackWeatherForecast>>>
{
    [Key(0)]
    public int Count { get; init; }
}
