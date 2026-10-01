namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/msgpack/bytes-upload", EVerbs.Post)]
[MessagePackObject]
public class MessagePackBytesUploadContract : IContract<MessagePackBinaryResponse>
{
    [Key(0)]
    public string FileName { get; init; } = "upload.bin";

    [Key(1)]
    public string MediaType { get; init; } = "application/octet-stream";

    [Key(2)]
    public byte[] Content { get; init; } = Array.Empty<byte>();
}
