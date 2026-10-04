namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/compress/binary", EVerbs.Post)]
[ContractCompression(K4os.Compression.LZ4.LZ4Level.L00_FAST)]
public class TestCompressBinaryContract : IContract<byte[]>
{
    public byte[] Content { get; init; } = [];

    public string FileName { get; init; } = "upload.bin";
}
