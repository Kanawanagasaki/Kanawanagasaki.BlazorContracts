namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using K4os.Compression.LZ4;
using MessagePack;

[Contract("/api/test/richest/{FolderId}", EVerbs.Post)]
[MessagePackObject]
[ContractCompression(LZ4Level.L03_HC)]
public class MostComplexPostContract : IContract<ContractFile>
{
    [Key(0)] public int Id { get; init; }
    [Key(1)] public string Tag { get; init; }
    [Key(2)] public int FolderId { get; init; }
    [Key(3)] public byte[] Chunk { get; init; }
    [Key(4)] public ContractFile? Attachment { get; init; }

    public MostComplexPostContract()
    {
        Tag = string.Empty;
        Chunk = Array.Empty<byte>();
    }
}
