namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;

/// <summary>
/// Demonstrates MessagePack contract + MessagePack response with a streaming ContractFile
/// property. The request is sent as multipart/form-data (because ContractFile cannot be
/// buffered in the MessagePack body — it's a stream), but the contract metadata part inside
/// the multipart is MessagePack-encoded rather than JSON.
/// </summary>
public class MessagePackStreamUploadHandler(AppStore store) : IContractHandler<MessagePackStreamUploadContract, MessagePackBinaryResponse>
{
    private readonly AppStore _store = store;

    public async Task<ContractResult<MessagePackBinaryResponse>> HandleAsync(MessagePackStreamUploadContract contract, CancellationToken ct = default)
    {
        if (contract.Content is null)
            return new ContractResult<MessagePackBinaryResponse>(StatusCodes.Status400BadRequest, "No file content received.");

        await using var ms = new MemoryStream();
        await contract.Content.Stream.CopyToAsync(ms, ct);
        var bytes = ms.ToArray();

        var (id, file) = _store.StoreFile(contract.FileName, contract.MediaType, bytes);

        var response = new MessagePackBinaryResponse
        {
            Id = id,
            FileName = file.FileName,
            MediaType = file.MediaType,
            UploadedAt = file.UploadedAt,
            Content = file.Content,
            Sha256 = file.Sha256
        };

        return new ContractResult<MessagePackBinaryResponse>(response);
    }
}
