namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/compress", EVerbs.Post)]
[MessagePackObject]
[ContractCompression]
public class MessagePackCompressContract : IContract<MessagePackWeatherForecast>
{
    [Key(0)]
    public int Seed { get; init; }

    [Key(1)]
    public int Days { get; init; }

    [Key(2)]
    public string City { get; init; }

    public MessagePackCompressContract()
    {
        City = string.Empty;
    }
}
