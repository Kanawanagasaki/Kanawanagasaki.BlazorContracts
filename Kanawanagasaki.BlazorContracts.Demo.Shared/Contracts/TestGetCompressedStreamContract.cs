namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using K4os.Compression.LZ4;

[Contract("/api/test/compressed-stream", EVerbs.Get)]
[ContractCompression(LZ4Level.L00_FAST)]
public class TestGetCompressedStreamContract : IContract<Stream> { }
