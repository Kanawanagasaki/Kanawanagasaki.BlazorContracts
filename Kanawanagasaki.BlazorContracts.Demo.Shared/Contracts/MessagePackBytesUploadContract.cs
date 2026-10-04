namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/bytes-upload", EVerbs.Post)]
[MessagePackObject]
public class MessagePackBytesUploadContract : IContract<MessagePackBinaryResponse>
{
    [Key(0)]
    public string FileName { get; init; }

    [Key(1)]
    public string MediaType { get; init; }

    [Key(2)]
    public byte[] Content { get; init; }

    public MessagePackBytesUploadContract()
    {
        FileName = "upload.bin";
        MediaType = "application/octet-stream";
        Content = Array.Empty<byte>();
    }
}
