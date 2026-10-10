namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;

public static partial class ContractBody
{
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
            await using var encoder = ContractLz4.CreateEncoderStream(stream, _level, leaveOpen: true);
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
}
