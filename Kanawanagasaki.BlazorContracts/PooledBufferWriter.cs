namespace Kanawanagasaki.BlazorContracts;

using System.Buffers;

public sealed class PooledBufferWriter : IBufferWriter<byte>, IDisposable
{
    private byte[] _buffer;
    private int _written;
    private bool _disposed;

    public PooledBufferWriter(int initialCapacity = 256)
        => _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);

    public int WrittenLength
        => _written;

    public ReadOnlySpan<byte> WrittenSpan
        => _buffer.AsSpan(0, _written);

    public ReadOnlyMemory<byte> WrittenMemory
        => _buffer.AsMemory(0, _written);

    public Span<byte> GetSpan(int sizeHint)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsSpan(_written);
    }

    public Memory<byte> GetMemory(int sizeHint)
    {
        EnsureCapacity(sizeHint);
        return _buffer.AsMemory(_written);
    }

    public void Advance(int count)
        => _written += count;

    public void Reset()
        => _written = 0;

    private void EnsureCapacity(int sizeHint)
    {
        if (sizeHint == 0)
            sizeHint = 1;
        if (_buffer.Length - _written < sizeHint)
        {
            var required = _written + sizeHint;
            var bigger = ArrayPool<byte>.Shared.Rent(Math.Max(required, _buffer.Length * 2));
            System.Buffer.BlockCopy(_buffer, 0, bigger, 0, _written);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = bigger;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
        _buffer = [];
    }
}
