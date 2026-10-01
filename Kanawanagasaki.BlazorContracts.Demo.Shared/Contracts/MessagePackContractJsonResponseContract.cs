namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/mixed-json-response", EVerbs.Post)]
[MessagePackObject]
public class MessagePackContractJsonResponseContract : IContract<SimpleTestResponse>
{
    [Key(0)]
    public string Input { get; init; } = string.Empty;

    [Key(1)]
    public int Echo { get; init; }
}
