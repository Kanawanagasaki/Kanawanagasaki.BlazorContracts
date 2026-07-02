namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/files/{Id}/stream", EVerbs.Get)]
public class FileDownloadStreamContract : IContract<Stream>
{
    public int Id { get; set; }
}
