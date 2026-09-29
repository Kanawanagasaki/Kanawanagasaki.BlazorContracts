namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

/// <summary>
/// Contract is marked with [MessagePackObject] but the response is a plain JSON model. The
/// contract body is sent as MessagePack; the response ContractResult&lt;SimpleTestResponse&gt;
/// is returned as JSON. Demonstrates the "MessagePack contract / JSON response" mixed mode.
/// </summary>
[Contract("/api/msgpack/mixed-json-response", EVerbs.Post)]
[MessagePackObject]
public class MessagePackContractJsonResponseContract : IContract<SimpleTestResponse>
{
    [Key(0)]
    public string Input { get; set; } = string.Empty;

    [Key(1)]
    public int Echo { get; set; }
}
