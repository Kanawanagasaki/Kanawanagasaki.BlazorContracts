namespace Kanawanagasaki.BlazorContracts;

using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Text;

public static class ContractHeaders
{
    public const string JsonMediaType = "application/json";
    public const string JsonUtf8MediaType = "application/json; charset=utf-8";
    public const string MessagePackMediaType = "application/x-msgpack";
    public const string OctetStreamMediaType = "application/octet-stream";
    public const string SseMediaType = "text/event-stream";

    public const string XOriginalSizeHeader = "X-Original-Size";
    public const string XContentEncodingHeader = "X-Content-Encoding";
    public const string XContentEncodingHeaderLz4 = "lz4";
    public const string XContentTypeHeader = "X-Content-Type";
    public const string ContentDispositionHeader = "Content-Disposition";

    public static bool IsJson(string? mediaType)
        => GetMediaType(mediaType).Equals(JsonMediaType, StringComparison.OrdinalIgnoreCase)
        || GetMediaType(mediaType).Equals(JsonUtf8MediaType, StringComparison.OrdinalIgnoreCase);

    public static bool IsMessagePack(string? mediaType)
        => GetMediaType(mediaType).Equals(MessagePackMediaType, StringComparison.OrdinalIgnoreCase);

    public static bool IsBinary(string? mediaType)
        => GetMediaType(mediaType).Equals(OctetStreamMediaType, StringComparison.OrdinalIgnoreCase);

    public static bool IsMultipart(string? mediaType)
        => mediaType is not null
        && mediaType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);

    public static bool IsSse(string? mediaType)
        => GetMediaType(mediaType).Equals(SseMediaType, StringComparison.OrdinalIgnoreCase);

    private static ReadOnlySpan<char> GetMediaType(string? mediaType)
    {
        if (mediaType is null)
            return default;

        var span = mediaType.AsSpan();
        var separatorIndex = span.IndexOf(';');
        if (separatorIndex >= 0)
            span = span[..separatorIndex];
        return span.Trim();
    }

    public static bool IsLz4(IDictionary<string, StringValues>? headers)
        => headers is not null
        && headers.TryGetValue(XContentEncodingHeader, out var encodingValues)
        && encodingValues.Any(IsLz4);

    public static bool IsLz4(string? headerValue)
        => headerValue is not null && headerValue.AsSpan().Trim().Equals(XContentEncodingHeaderLz4, StringComparison.OrdinalIgnoreCase);

    public static bool IsLz4Response(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(XContentEncodingHeader, out var values) && values.Any(IsLz4))
            return true;
        if (response.Content?.Headers.TryGetValues(XContentEncodingHeader, out var contentValues) == true && contentValues.Any(IsLz4))
            return true;
        return false;
    }

    public static string? GetXContentType(HttpResponseMessage response)
    {
        IEnumerable<string>? values;
        if (response.Content.Headers.TryGetValues(XContentTypeHeader, out values))
            return values.FirstOrDefault();
        if (response.Headers.TryGetValues(XContentTypeHeader, out values))
            return values.FirstOrDefault();
        return null;
    }

    public static string? GetContentDisposition(HttpResponseMessage response)
    {
        IEnumerable<string>? values;
        if (response.Content.Headers.TryGetValues(ContentDispositionHeader, out values))
            return string.Join(", ", values);
        if (response.Headers.TryGetValues(ContentDispositionHeader, out values))
            return string.Join(", ", values);
        return null;
    }

    public static string CreateAttachmentDisposition(string? fileName)
    {
        var name = string.IsNullOrEmpty(fileName) ? "file" : fileName;

        var fallback = new StringBuilder(name.Length);
        foreach (var c in name.Normalize(NormalizationForm.FormD))
        {
            if (c is '"' or '\\')
                fallback.Append('\\').Append(c);
            else if (0x20 <= c && c <= 0x7E)
                fallback.Append(c);
        }

        if (0 == fallback.Length)
            fallback.Append("file");

        return $"attachment; filename=\"{fallback}\"; filename*=UTF-8''{Uri.EscapeDataString(name)}";
    }

    public static bool IsAttachment(string? contentDisposition)
        => ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition)
        && disposition.DispositionType.Equals("attachment", StringComparison.OrdinalIgnoreCase);

    public static string? GetFileNameFromDisposition(string? contentDisposition)
    {
        if (!ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition))
            return null;

        if (disposition.FileNameStar.HasValue && !string.IsNullOrWhiteSpace(disposition.FileNameStar.Value))
            return disposition.FileNameStar.Value;

        if (disposition.FileName.HasValue && !string.IsNullOrWhiteSpace(disposition.FileName.Value))
            return disposition.FileName.Value;

        return null;
    }

    public static long? GetXOriginalSize(HttpResponseMessage response)
    {
        IEnumerable<string>? values;
        if (response.Content.Headers.TryGetValues(XOriginalSizeHeader, out values))
        {
            foreach (var val in values)
            {
                if (long.TryParse(val, out var len))
                    return len;
            }
        }

        if (response.Headers.TryGetValues(XOriginalSizeHeader, out values))
        {
            foreach (var val in values)
            {
                if (long.TryParse(val, out var len))
                    return len;
            }
        }

        return null;
    }

    public static long? GetXOriginalSize(IDictionary<string, StringValues>? headers)
    {
        if (headers is null)
            return null;

        if (headers.TryGetValue(XOriginalSizeHeader, out var values))
        {
            foreach (var val in values)
            {
                if (long.TryParse(val, out var len))
                    return len;
            }
        }

        return null;
    }
}
