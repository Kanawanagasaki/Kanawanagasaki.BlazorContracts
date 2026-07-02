namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class FileUploadBytesHandler(AppStore store) : IContractHandler<FileUploadBytesContract, FileResult>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<FileResult>> HandleAsync(FileUploadBytesContract contract, CancellationToken ct = default)
    {
        if (contract.Content is null || contract.Content.Length == 0)
            return Task.FromResult(new ContractResult<FileResult>(StatusCodes.Status400BadRequest, "No file content received."));

        var (id, file) = _store.StoreFile(contract.FileName, contract.MediaType, contract.Content);

        var result = new FileResult
        {
            Id = id,
            FileName = file.FileName,
            Length = file.Length,
            MediaType = file.MediaType,
            UploadedAt = file.UploadedAt,
            Sha256 = file.Sha256
        };

        return Task.FromResult(new ContractResult<FileResult>(result));
    }
}
