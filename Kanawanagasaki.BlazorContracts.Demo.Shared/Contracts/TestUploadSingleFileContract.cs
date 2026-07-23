namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/upload/single", EVerbs.Post)]
public class TestUploadSingleFileContract : IContract<FileUploadResult>
{
    public ContractFile? File { get; set; }
}
