namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System.Net.Http.Headers;
using System.Text.Json;

public static class ContractBody
{
    public static HttpContent CreateJson<T>(T value, JsonSerializerOptions? jsonOptions = null, LZ4Level? compressionLevel = null)
        => new JsonContent<T>(value, jsonOptions, compressionLevel);

    public static HttpContent CreateMessagePack<T>(T value, LZ4Level? compressionLevel = null)
        => new MessagePackContent<T>(value, compressionLevel);

    public static HttpContent CreateBinary(byte[] bytes, string mediaType, LZ4Level? compressionLevel = null)
    {
        if (compressionLevel.HasValue)
        {
            var ms = new MemoryStream(bytes);
            var content = new Lz4StreamContent(ms, compressionLevel.Value);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            content.Headers.TryAddWithoutValidation(ContractHeaders.XContentEncodingHeader, ContractHeaders.XContentEncodingHeaderLz4);
            return content;
        }
        else
        {
            var content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
            return content;
        }
    }

    public static HttpContent CreateFile(ContractFile file, LZ4Level? compressionLevel = null)
    {
        if (compressionLevel.HasValue)
        {
            var content = new Lz4StreamContent(file.Stream, compressionLevel.Value);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.MediaType);
            content.Headers.TryAddWithoutValidation(ContractHeaders.XContentEncodingHeader, ContractHeaders.XContentEncodingHeaderLz4);
            return content;
        }
        else
        {
            var streamContent = new StreamContent(file.Stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.MediaType);
            return streamContent;
        }
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
                Headers.TryAddWithoutValidation(ContractHeaders.XContentEncodingHeader, ContractHeaders.XContentEncodingHeaderLz4);
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

            await using var encoder = LZ4Stream.Encode(stream, _compressionLevel.Value, leaveOpen: true);
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

    private sealed class JsonContent<T> : SerializerContent<T>
    {
        private readonly JsonSerializerOptions? _jsonOptions;

        public JsonContent(T value, JsonSerializerOptions? jsonOptions, LZ4Level? compressionLevel) : base(value, ContractHeaders.JsonMediaType, compressionLevel)
        {
            _jsonOptions = jsonOptions;
        }

        protected override Task SerializeAsync(Stream stream, CancellationToken ct)
            => JsonSerializer.SerializeAsync(stream, _value, _jsonOptions, ct);
    }

    private sealed class Lz4StreamContent : HttpContent
    {
        private readonly Stream _source;
        private readonly LZ4Level _level;

        public Lz4StreamContent(Stream source, LZ4Level level)
        {
            _source = source;
            _level = level;
        }

        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context, CancellationToken ct)
        {
            await using var encoder = LZ4Stream.Encode(stream, _level, leaveOpen: true);
            await _source.CopyToAsync(encoder, ct);
            await encoder.FlushAsync(ct);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _source.Dispose();
            base.Dispose(disposing);
        }
    }

    private sealed class MessagePackContent<T> : SerializerContent<T>
    {
        public MessagePackContent(T value, LZ4Level? compressionLevel) : base(value, ContractHeaders.MessagePackMediaType, compressionLevel) { }

        protected override Task SerializeAsync(Stream stream, CancellationToken ct)
            => BlazorContractsMessagePack.SerializeAsync(stream, _value, ct);
    }
}
