namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class FileUploadResult
{
    public string FileName { get; set; } = string.Empty;
    public long Length { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
}
