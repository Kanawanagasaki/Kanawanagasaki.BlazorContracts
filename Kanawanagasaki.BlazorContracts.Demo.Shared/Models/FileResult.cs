namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class FileResult
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Length { get; set; }
    public string MediaType { get; set; } = "application/octet-stream";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string Sha256 { get; set; } = string.Empty;
}
