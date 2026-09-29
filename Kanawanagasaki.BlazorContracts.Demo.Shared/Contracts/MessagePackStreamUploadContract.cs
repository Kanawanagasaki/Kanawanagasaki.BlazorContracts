namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

/// <summary>
/// Both contract and response are MessagePack-marked. The contract contains a ContractFile
/// (streaming) property, so the request must be sent as multipart/form-data even though the
/// contract body itself is MessagePack-encoded. The response is a single MessagePack body
/// containing the binary content directly (no multipart response, because MessagePack encodes
/// byte[] natively).
/// </summary>
[Contract("/api/msgpack/stream-upload", EVerbs.Post)]
[MessagePackObject]
public class MessagePackStreamUploadContract : IContract<MessagePackBinaryResponse>
{
    [Key(0)]
    public string FileName { get; set; } = "upload.bin";

    [Key(1)]
    public string MediaType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Streaming file — shipped as a separate multipart part, because we never want to
    /// fully materialize it on the client before sending.
    /// </summary>
    [Key(2)]
    public ContractFile? Content { get; set; }
}
