namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/weather", EVerbs.Post)]
[MessagePackObject]
public class MessagePackWeatherContract : IContract<MessagePackWeatherForecast>
{
    [Key(0)]
    public int Seed { get; init; }

    [Key(1)]
    public int Days { get; init; }

    [Key(2)]
    public byte[] Payload { get; init; } = Array.Empty<byte>();
}
