namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using K4os.Compression.LZ4;

[Contract("/api/test/compressed-envelope", EVerbs.Get)]
[ContractCompression(LZ4Level.L03_HC)]
public class TestGetCompressedEnvelopeContract : IContract<SimpleTestResponse>
{
    public string Echo { get; init; } = string.Empty;
}
