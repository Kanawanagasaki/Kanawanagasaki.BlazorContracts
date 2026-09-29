namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

/// <summary>
/// Contract is plain JSON (no [MessagePackObject]) but the response type is marked with
/// [MessagePackObject]. The contract body is sent as JSON; the response ContractResult&lt;...&gt;
/// is returned as a MessagePack body. Demonstrates the "JSON contract / MessagePack response"
/// mixed mode.
/// </summary>
[Contract("/api/msgpack/json-contract-mp-response", EVerbs.Post)]
public class JsonContractMessagePackResponseContract : IContract<MessagePackWeatherForecast>
{
    public int Seed { get; set; }
    public int Days { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
