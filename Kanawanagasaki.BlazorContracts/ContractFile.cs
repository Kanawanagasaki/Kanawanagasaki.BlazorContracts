namespace Kanawanagasaki.BlazorContracts;


public sealed class ContractFile : IAsyncDisposable, IDisposable
{
    public Stream Stream { get; }
    public string FileName { get; }
    public string MediaType { get; }

    public long Length { get; }

    public bool LeaveOpen { get; }

    public ContractFile(Stream stream, string fileName, string mediaType, long length = -1, bool leaveOpen = false)
    {
        Stream = stream;
        FileName = fileName;
        MediaType = mediaType;
        Length = length;
        LeaveOpen = leaveOpen;
    }

    public async ValueTask DisposeAsync()
    {
        if (!LeaveOpen)
            await Stream.DisposeAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (!LeaveOpen)
            Stream.Dispose();
    }
}
