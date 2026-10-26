namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using K4os.Compression.LZ4;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

public enum TestQueryScope
{
    Local = 1,
    Regional = 2,
    Global = 3
}

[Contract("/api/test/msgpack-compressed/{Id}", EVerbs.Get)]
[MessagePackObject]
[ContractCompression(LZ4Level.L03_HC)]
public class TestGetMsgpackCompressedComplexContract : IContract<ComplexResponse>
{
    [Key(0)]
    public int Id { get; init; }

    [Key(1)]
    public string? Tag { get; init; }

    [Key(2)]
    public int? Limit { get; init; }

    [Key(3)]
    public int Offset { get; init; }

    [Key(4)]
    public TestQueryScope Scope { get; init; }
}
