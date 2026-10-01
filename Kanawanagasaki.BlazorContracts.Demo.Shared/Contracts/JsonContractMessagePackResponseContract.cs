namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/msgpack/json-contract-mp-response", EVerbs.Post)]
public class JsonContractMessagePackResponseContract : IContract<MessagePackWeatherForecast>
{
    public int Seed { get; init; }
    public int Days { get; init; }
    public byte[] Payload { get; init; } = Array.Empty<byte>();
}
