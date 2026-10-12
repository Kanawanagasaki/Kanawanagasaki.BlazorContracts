namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/test/msgpack-envelope", EVerbs.Get)]
[MessagePackObject]
public class TestGetMsgpackEnvelopeContract : IContract<SimpleTestResponse>
{
    [Key(0)]
    public string Echo { get; init; }

    public TestGetMsgpackEnvelopeContract()
    {
        Echo = string.Empty;
    }
}
