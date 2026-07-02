namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class FileUploadStreamHandler(AppStore store) : IContractHandler<FileUploadStreamContract, FileResult>
{
    private readonly AppStore _store = store;

    public async Task<ContractResult<FileResult>> HandleAsync(FileUploadStreamContract contract, CancellationToken ct = default)
    {
        if (contract.Content is null)
            return new ContractResult<FileResult>(StatusCodes.Status400BadRequest, "No file content received.");

        await using var ms = new MemoryStream();
        await contract.Content.Stream.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var fileName = string.IsNullOrWhiteSpace(contract.FileName)
            ? contract.Content.FileName
            : contract.FileName;
        var mediaType = string.IsNullOrWhiteSpace(contract.Content.MediaType)
            ? "application/octet-stream"
            : contract.Content.MediaType;

        var (id, file) = _store.StoreFile(fileName, mediaType, bytes);

        var result = new FileResult
        {
            Id = id,
            FileName = file.FileName,
            Length = file.Length,
            MediaType = file.MediaType,
            UploadedAt = file.UploadedAt,
            Sha256 = file.Sha256
        };

        return new ContractResult<FileResult>(result);
    }
}
