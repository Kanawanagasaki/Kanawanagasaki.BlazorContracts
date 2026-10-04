namespace Kanawanagasaki.BlazorContracts;

public sealed class ContractFile
{
    public Stream Stream { get; }
    public string FileName { get; }
    public string MediaType { get; }

    public long Length { get; }

    public ContractFile(Stream stream, string fileName, string mediaType, long length = -1)
    {
        Stream = stream;
        FileName = fileName;
        MediaType = mediaType;
        Length = length;
    }
}
