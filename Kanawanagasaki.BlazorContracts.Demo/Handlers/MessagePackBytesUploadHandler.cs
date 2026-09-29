namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;

/// <summary>
/// Demonstrates MessagePack contract + MessagePack response with an inline byte[] payload.
/// Because MessagePack encodes byte[] natively as a bin type, the entire contract body —
/// including the binary payload — is shipped as a single MessagePack body. No multipart.
/// </summary>
public class MessagePackBytesUploadHandler(AppStore store) : IContractHandler<MessagePackBytesUploadContract, MessagePackBinaryResponse>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<MessagePackBinaryResponse>> HandleAsync(MessagePackBytesUploadContract contract, CancellationToken ct = default)
    {
        if (contract.Content is null || contract.Content.Length == 0)
            return Task.FromResult(new ContractResult<MessagePackBinaryResponse>(StatusCodes.Status400BadRequest, "No file content received."));

        var (id, file) = _store.StoreFile(contract.FileName, contract.MediaType, contract.Content);

        var response = new MessagePackBinaryResponse
        {
            Id = id,
            FileName = file.FileName,
            MediaType = file.MediaType,
            UploadedAt = file.UploadedAt,
            Content = file.Content,
            Sha256 = file.Sha256
        };

        return Task.FromResult(new ContractResult<MessagePackBinaryResponse>(response));
    }
}
