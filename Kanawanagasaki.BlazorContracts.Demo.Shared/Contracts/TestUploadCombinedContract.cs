namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/upload/combined", EVerbs.Post)]
public class TestUploadCombinedContract : IContract<FileUploadResult[]>
{
    public byte[]? Data { get; set; }
    public ContractFile? File1 { get; set; }
    public ContractFile? File2 { get; set; }
}
