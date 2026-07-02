namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class FileDownloadStreamHandler(AppStore store) : IContractHandler<FileDownloadStreamContract, Stream>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<Stream>> HandleAsync(FileDownloadStreamContract contract, CancellationToken ct = default)
    {
        var file = _store.GetFileById(contract.Id);
        if (file is null)
            return Task.FromResult(new ContractResult<Stream>(StatusCodes.Status404NotFound, $"File with id={contract.Id} not found."));

        var stream = new MemoryStream(file.Content, writable: false);
        return Task.FromResult(new ContractResult<Stream>(stream));
    }
}
