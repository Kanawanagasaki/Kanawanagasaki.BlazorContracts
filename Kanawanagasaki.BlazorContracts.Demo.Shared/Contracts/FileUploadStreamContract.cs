namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/files/stream", EVerbs.Post)]
public class FileUploadStreamContract : IContract<FileResult>
{
    public ContractFile? Content { get; init; }
    public string FileName { get; init; } = "upload.bin";
}
