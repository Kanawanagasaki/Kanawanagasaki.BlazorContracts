namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using K4os.Compression.LZ4;

[Contract("/api/files/{Id}/compressed-file", EVerbs.Get)]
[ContractCompression(LZ4Level.L00_FAST)]
public class FileDownloadCompressedFileContract : IContract<ContractFile>
{
    public int Id { get; init; }
}
