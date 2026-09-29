namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

/// <summary>
/// Both contract and response are MessagePack-marked. The contract carries a byte[] payload
/// inline (not a stream). Because MessagePack encodes byte[] natively, no multipart is needed:
/// the entire contract — including the binary payload — is sent as a single MessagePack body.
/// This contrasts with the JSON path, where a byte[] property would force a multipart request
/// to avoid base64 bloat.
/// </summary>
[Contract("/api/msgpack/bytes-upload", EVerbs.Post)]
[MessagePackObject]
public class MessagePackBytesUploadContract : IContract<MessagePackBinaryResponse>
{
    [Key(0)]
    public string FileName { get; set; } = "upload.bin";

    [Key(1)]
    public string MediaType { get; set; } = "application/octet-stream";

    [Key(2)]
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
