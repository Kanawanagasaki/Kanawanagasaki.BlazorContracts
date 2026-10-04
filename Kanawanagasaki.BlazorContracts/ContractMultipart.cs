namespace Kanawanagasaki.BlazorContracts;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

public static class ContractMultipart
{
    public static int SpoolMemoryThreshold { get; set; } = 1024 * 1024;

    public sealed class Part : IDisposable, IAsyncDisposable
    {
        private readonly Stream _body;
        private readonly bool _compressed;
        private Stream? _decodingStream;

        public string? Name { get; internal set; }
        public string? FileName { get; internal set; }
        public string? ContentType { get; }

        public long Length { get; }

        public Stream Stream
        {
            get
            {
                if (_compressed && _decodingStream is null)
                    _decodingStream = ContractLz4.CreateDecoderStream(_body, leaveOpen: true);
                return _decodingStream ?? _body;
            }
        }

        internal Part(Stream body, bool compressed, string? contentType, long length)
        {
            _body = body;
            _compressed = compressed;
            ContentType = contentType;
            Length = compressed ? -1 : length;
        }

        public void ApplyContentDisposition(string? contentDisposition)
        {
            if (string.IsNullOrEmpty(contentDisposition))
                return;

            if (!ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition))
                return;

            if (disposition.Name.HasValue)
                Name = disposition.Name.Value;
            if (disposition.FileName.HasValue)
                FileName = disposition.FileName.Value;
        }

        public void Dispose()
        {
            _decodingStream?.Dispose();
            _body.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_decodingStream is not null)
                await _decodingStream.DisposeAsync();
            await _body.DisposeAsync();
        }
    }

    private static string? GetBoundary(string? contentType)
    {
        if (!MediaTypeHeaderValue.TryParse(contentType, out var mediaType))
            return null;

        var boundaryParameter = mediaType.Parameters.FirstOrDefault(x => x.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase));
        if (boundaryParameter is null || !boundaryParameter.Value.HasValue)
            return null;

        var value = boundaryParameter.Value.Value.Trim().Trim('"');
        return !string.IsNullOrWhiteSpace(value) ? value : null;
    }

    public static async Task<Dictionary<string, Part>?> ParseAsync(Stream body, string? contentType, CancellationToken ct = default)
    {
        var boundary = GetBoundary(contentType);
        if (boundary is null)
            return null;

        var reader = new MultipartReader(boundary, body);
        var parts = new Dictionary<string, Part>();

        try
        {
            while (await reader.ReadNextSectionAsync(ct) is { } section)
            {
                if (section.Body is null)
                    continue;

                var compressed = section.Headers is not null
                    && section.Headers.TryGetValue(ContractLz4.HeaderName, out var encodingValues)
                    && encodingValues.Any(ContractLz4.IsLz4);

                var spooled = new FileBufferingReadStream(section.Body, SpoolMemoryThreshold);
                Part part;
                try
                {
                    await spooled.DrainAsync(ct);
                    spooled.Position = 0;
                    part = new Part(spooled, compressed, section.ContentType, spooled.Length);
                }
                catch
                {
                    await spooled.DisposeAsync();
                    throw;
                }

                part.ApplyContentDisposition(section.ContentDisposition);
                part.Name ??= part.FileName ?? Guid.NewGuid().ToString("N");
                parts[part.Name] = part;
            }

            return parts;
        }
        catch
        {
            foreach (var part in parts.Values)
                await part.DisposeAsync();
            throw;
        }
    }
}
