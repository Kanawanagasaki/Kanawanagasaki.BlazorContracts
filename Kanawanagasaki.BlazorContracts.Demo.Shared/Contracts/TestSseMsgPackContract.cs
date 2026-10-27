namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/test/sse/msgpack", EVerbs.Post)]
[MessagePackObject]
public class TestSseMsgPackContract : IContract<IAsyncEnumerable<SseItem<MessagePackWeatherForecast>>>
{
    [Key(0)]
    public int Count { get; init; }
}
