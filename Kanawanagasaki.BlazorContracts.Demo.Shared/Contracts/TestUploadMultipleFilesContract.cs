namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/upload/multiple", EVerbs.Post)]
public class TestUploadMultipleFilesContract : IContract<FileUploadResult[]>
{
    public ContractFile? File1 { get; set; }
    public ContractFile? File2 { get; set; }
}
