namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/files/{Id}/bytes", EVerbs.Get)]
public class FileDownloadBytesContract : IContract<byte[]>
{
    public int Id { get; set; }
}
