namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;
using System.Text.Json;

public static partial class ContractBody
{
    private sealed class JsonContent<T> : SerializerContent<T>
    {
        private readonly JsonSerializerOptions? _jsonOptions;

        public JsonContent(T value, JsonSerializerOptions? jsonOptions, LZ4Level? compressionLevel) : base(value, ContractMediaTypes.Json, compressionLevel)
        {
            _jsonOptions = jsonOptions;
        }

        protected override Task SerializeAsync(Stream stream, CancellationToken ct)
            => JsonSerializer.SerializeAsync(stream, _value, _jsonOptions, ct);
    }
}
