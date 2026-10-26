namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4.Streams;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Runtime.CompilerServices;

public static class ContractMultipart
{
    public const int SpoolMemoryThreshold = 1024 * 1024;

    public sealed class Part : IDisposable, IAsyncDisposable
    {
        public Stream Stream { get; }

        public string? Name { get; internal set; }
        public string? FileName { get; internal set; }
        public string? DispositionType { get; internal set; }
        public string? ContentType { get; }
        public long Length { get; }

        internal Part(Stream body, string? contentType, long length)
        {
            Stream = body;
            ContentType = contentType;
            Length = length;
        }

        public void ApplyContentDisposition(string? contentDisposition)
        {
            if (string.IsNullOrEmpty(contentDisposition))
                return;

            if (!ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition))
                return;

            if (disposition.DispositionType.HasValue)
                DispositionType = disposition.DispositionType.Value;

            if (disposition.Name.HasValue)
                Name = disposition.Name.Value;
            if (disposition.FileNameStar.HasValue && !string.IsNullOrWhiteSpace(disposition.FileNameStar.Value))
                FileName = disposition.FileNameStar.Value;
            else if (disposition.FileName.HasValue)
                FileName = disposition.FileName.Value;
        }

        public void Dispose()
        {
            Stream.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await Stream.DisposeAsync();
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

    public static async IAsyncEnumerable<Part> ParseAsync(Stream body, string? contentType, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var boundary = GetBoundary(contentType);
        if (boundary is null)
            yield break;

        var reader = new MultipartReader(boundary, body);

        while (await reader.ReadNextSectionAsync(ct) is { } section)
        {
            if (section.Body is null)
                continue;

            var spooled = new FileBufferingReadStream(section.Body, SpoolMemoryThreshold);
            Part part;
            try
            {
                await spooled.DrainAsync(ct);
                spooled.Position = 0;

                var compressed = ContractHeaders.IsLz4(section.Headers);
                var contentLength = compressed ? ContractHeaders.GetXOriginalSize(section.Headers) : spooled.Length;

                part = new Part(compressed ? LZ4Stream.Decode(spooled) : spooled, section.ContentType, contentLength ?? -1);
            }
            catch
            {
                await spooled.DisposeAsync();
                throw;
            }

            part.ApplyContentDisposition(section.ContentDisposition);

            yield return part;
        }
    }
}
