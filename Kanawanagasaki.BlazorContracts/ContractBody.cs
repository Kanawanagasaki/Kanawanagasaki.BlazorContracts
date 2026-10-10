namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;
using System.Net.Http.Headers;
using System.Text.Json;

public static partial class ContractBody
{
    public static HttpContent CreateJson<T>(T value, JsonSerializerOptions? jsonOptions = null, LZ4Level? compressionLevel = null)
        => new JsonContent<T>(value, jsonOptions, compressionLevel);

    public static HttpContent CreateMessagePack<T>(T value, LZ4Level? compressionLevel = null)
        => new MessagePackContent<T>(value, compressionLevel);

    public static ByteArrayContent CreateBinary(byte[] bytes, string mediaType, LZ4Level? compressionLevel = null)
    {
        if (compressionLevel is not null)
            bytes = ContractLz4.Compress(bytes, compressionLevel.Value);

        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        if (compressionLevel is not null)
            content.Headers.TryAddWithoutValidation(ContractLz4.HeaderName, ContractLz4.HeaderValue);
        return content;
    }

    public static HttpContent CreateFile(ContractFile file, LZ4Level? compressionLevel = null)
    {
        if (compressionLevel.HasValue)
        {
            var content = new Lz4StreamContent(file.Stream, compressionLevel.Value);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.MediaType);
            content.Headers.TryAddWithoutValidation(ContractLz4.HeaderName, ContractLz4.HeaderValue);
            return content;
        }

        var streamContent = new StreamContent(file.Stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.MediaType);
        return streamContent;
    }

    private abstract class SerializerContent<T> : HttpContent
    {
        protected readonly T _value;
        private readonly LZ4Level? _compressionLevel;

        protected SerializerContent(T value, string mediaType, LZ4Level? compressionLevel)
        {
            _value = value;
            _compressionLevel = compressionLevel;
            Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            if (compressionLevel is not null)
                Headers.TryAddWithoutValidation(ContractLz4.HeaderName, ContractLz4.HeaderValue);
        }

        protected sealed override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected sealed override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context, CancellationToken ct)
        {
            if (_compressionLevel is null)
            {
                await SerializeAsync(stream, ct);
                return;
            }

            await using var encoder = ContractLz4.CreateEncoderStream(stream, _compressionLevel.Value, leaveOpen: true);
            await SerializeAsync(encoder, ct);
            await encoder.FlushAsync(ct);
        }

        protected abstract Task SerializeAsync(Stream stream, CancellationToken ct);

        protected sealed override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
