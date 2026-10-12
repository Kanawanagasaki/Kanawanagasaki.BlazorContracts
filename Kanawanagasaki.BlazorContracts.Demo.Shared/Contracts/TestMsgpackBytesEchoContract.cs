namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using MessagePack;

[Contract("/api/test/msgpack-bytes-echo", EVerbs.Post)]
[MessagePackObject]
public class TestMsgpackBytesEchoContract : IContract<byte[]>
{
    [Key(0)]
    public byte[] Payload { get; init; }

    public TestMsgpackBytesEchoContract()
    {
        Payload = Array.Empty<byte>();
    }
}
