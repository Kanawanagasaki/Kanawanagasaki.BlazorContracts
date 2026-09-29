namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

using MessagePack;

/// <summary>
/// Response model for the MessagePack byte[] demo. The response type itself is MessagePackObject,
/// which triggers a single MessagePack body response (no multipart) even though it carries a
/// large byte[] payload — MessagePack encodes byte[] natively as a bin type, avoiding the
/// base64 inflation that JSON would impose.
/// </summary>
[MessagePackObject]
public class MessagePackBinaryResponse
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string FileName { get; set; } = string.Empty;

    [Key(2)]
    public string MediaType { get; set; } = "application/octet-stream";

    [Key(3)]
    public DateTime UploadedAt { get; set; }

    [Key(4)]
    public byte[] Content { get; set; } = Array.Empty<byte>();

    [Key(5)]
    public string Sha256 { get; set; } = string.Empty;
}
