namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class FileDownloadMsgpackFileHandler(AppStore store) : IContractHandler<FileDownloadMsgpackFileContract, ContractFile>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<ContractFile>> HandleAsync(FileDownloadMsgpackFileContract contract, CancellationToken ct = default)
    {
        var file = _store.GetFileById(contract.Id);
        if (file is null)
            return Task.FromResult(new ContractResult<ContractFile>(StatusCodes.Status404NotFound, $"File with id={contract.Id} not found."));

        var stream = new MemoryStream(file.Content, writable: false);
        return Task.FromResult(new ContractResult<ContractFile>(new ContractFile(stream, file.FileName, file.MediaType, file.Length)));
    }
}
