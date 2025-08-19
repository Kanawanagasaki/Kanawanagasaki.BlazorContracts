namespace Kanawanagasaki.BlazorContracts;

public sealed class ContractFile
{
    public Stream Stream { get; }
    public string FileName { get; }
    public string MediaType { get; }

    public long Length { get; } = -1;

    public ContractFile(Stream stream, string fileName, string mediaType)
    {
        Stream = stream;
        FileName = fileName;
        MediaType = mediaType;
    }

    public ContractFile(Stream stream, string fileName, string mediaType, long length)
    {
        Stream = stream;
        FileName = fileName;
        MediaType = mediaType;
        Length = length;
    }
}
