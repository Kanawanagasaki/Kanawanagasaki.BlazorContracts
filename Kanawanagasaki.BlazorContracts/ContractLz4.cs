namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;

public static class ContractLz4
{
    public const string HeaderName = "X-Content-Encoding";
    public const string HeaderValue = "lz4";

    public static bool IsLz4(string? headerValue)
        => headerValue is not null && headerValue.AsSpan().Trim().Equals(HeaderValue, StringComparison.OrdinalIgnoreCase);

    public static bool IsLz4Response(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(HeaderName, out var values) && values.Any(IsLz4))
            return true;
        if (response.Content?.Headers.TryGetValues(HeaderName, out var contentValues) == true && contentValues.Any(IsLz4))
            return true;
        return false;
    }

    public static byte[] Compress(byte[] data, LZ4Level level = LZ4Level.L00_FAST)
    {
        using var ms = new MemoryStream();

        using (var encoder = CreateEncoderStream(ms, level))
            encoder.Write(data);

        return ms.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var decoder = CreateDecoderStream(ms);
        using var outputMs = new MemoryStream();

        decoder.CopyTo(outputMs);
        return outputMs.ToArray();
    }

    public static Stream CreateEncoderStream(Stream destination, LZ4Level level = LZ4Level.L00_FAST, bool leaveOpen = false)
        => LZ4Stream.Encode(destination, level, leaveOpen: leaveOpen);

    public static Stream CreateDecoderStream(Stream source, bool leaveOpen = false)
        => LZ4Stream.Decode(source, leaveOpen: leaveOpen);
}
