namespace Kanawanagasaki.BlazorContracts;

public static class ContractMediaTypes
{
    public const string Json = "application/json";
    public const string JsonCharset = "application/json; charset=utf-8";
    public const string MessagePack = "application/x-msgpack";
    public const string Binary = "application/octet-stream";

    public static bool IsJson(string? mediaType)
        => GetMediaType(mediaType).Equals(Json, StringComparison.OrdinalIgnoreCase);

    public static bool IsMessagePack(string? mediaType)
        => GetMediaType(mediaType).Equals(MessagePack, StringComparison.OrdinalIgnoreCase);

    public static bool IsMultipart(string? mediaType)
        => mediaType is not null
        && mediaType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);

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
}
