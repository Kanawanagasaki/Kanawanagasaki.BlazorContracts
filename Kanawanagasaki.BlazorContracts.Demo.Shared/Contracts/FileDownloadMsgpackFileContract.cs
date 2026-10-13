namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using MessagePack;

[MessagePackObject]
[Contract("/api/files/{Id}/msgpack-file", EVerbs.Get)]
public class FileDownloadMsgpackFileContract : IContract<ContractFile>
{
    [Key(1)]
    public int Id { get; init; }
}
