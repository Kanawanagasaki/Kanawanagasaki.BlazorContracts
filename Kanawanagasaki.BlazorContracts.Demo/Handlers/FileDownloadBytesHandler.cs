namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class FileDownloadBytesHandler(AppStore store) : IContractHandler<FileDownloadBytesContract, byte[]>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<byte[]>> HandleAsync(FileDownloadBytesContract contract, CancellationToken ct = default)
    {
        var file = _store.GetFileById(contract.Id);
        if (file is null)
            return Task.FromResult(new ContractResult<byte[]>(StatusCodes.Status404NotFound, $"File with id={contract.Id} not found."));

        return Task.FromResult(new ContractResult<byte[]>(file.Content));
    }
}
