namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/files/bytes", EVerbs.Post)]
public class FileUploadBytesContract : IContract<FileResult>
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "upload.bin";
    public string MediaType { get; set; } = "application/octet-stream";
}
