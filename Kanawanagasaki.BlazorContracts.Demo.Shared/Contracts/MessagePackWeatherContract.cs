namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

/// <summary>
/// Both contract and response are marked with [MessagePackObject]. The request body and the
/// ContractResult&lt;MessagePackWeatherForecast&gt; response are both serialized as a single
/// MessagePack body (Content-Type: application/x-msgpack). No multipart, no base64.
/// </summary>
[Contract("/api/msgpack/weather", EVerbs.Post)]
[MessagePackObject]
public class MessagePackWeatherContract : IContract<MessagePackWeatherForecast>
{
    [Key(0)]
    public int Seed { get; set; }

    [Key(1)]
    public int Days { get; set; }

    [Key(2)]
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
