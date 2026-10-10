namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;

public static partial class ContractBody
{
    private sealed class MessagePackContent<T> : SerializerContent<T>
    {
        public MessagePackContent(T value, LZ4Level? compressionLevel) : base(value, ContractMediaTypes.MessagePack, compressionLevel) { }

        protected override Task SerializeAsync(Stream stream, CancellationToken ct)
            => BlazorContractsMessagePack.SerializeAsync(stream, _value, ct);
    }
}
