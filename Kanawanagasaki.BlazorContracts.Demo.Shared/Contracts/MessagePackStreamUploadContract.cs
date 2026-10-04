namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/stream-upload", EVerbs.Post)]
[MessagePackObject]
public class MessagePackStreamUploadContract : IContract<MessagePackBinaryResponse>
{
    [Key(0)]
    public string FileName { get; init; }

    [Key(1)]
    public string MediaType { get; init; }

    [Key(2)]
    public ContractFile? Content { get; init; }

    public MessagePackStreamUploadContract()
    {
        FileName = "upload.bin";
        MediaType = "application/octet-stream";
    }
}
