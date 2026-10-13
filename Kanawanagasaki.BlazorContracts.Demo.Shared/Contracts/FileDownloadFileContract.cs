namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/files/{Id}/file", EVerbs.Get)]
public class FileDownloadFileContract : IContract<ContractFile>
{
    public int Id { get; init; }
}
