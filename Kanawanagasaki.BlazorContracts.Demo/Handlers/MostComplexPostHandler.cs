namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Text;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class MostComplexPostHandler : IContractHandler<MostComplexPostContract, ContractFile>
{
    public async Task<ContractResult<ContractFile>> HandleAsync(MostComplexPostContract contract, CancellationToken ct = default)
    {
        var chunk = contract.Chunk ?? Array.Empty<byte>();

        using var attMs = new MemoryStream();
        if (contract.Attachment is not null)
            await contract.Attachment.Stream.CopyToAsync(attMs);

        var preamble = Encoding.UTF8.GetBytes($"folder={contract.FolderId},id={contract.Id},tag={contract.Tag}\n");
        var combined = new byte[preamble.Length + chunk.Length + attMs.Length];
        Array.Copy(preamble, 0, combined, 0, preamble.Length);
        Array.Copy(chunk, 0, combined, preamble.Length, chunk.Length);
        Array.Copy(attMs.ToArray(), 0, combined, preamble.Length + chunk.Length, attMs.Length);

        var fileName = $"{contract.FolderId}/{contract.Id}-{contract.Tag}.bin";
        return new ContractFile(new MemoryStream(combined, writable: false), fileName, "application/octet-stream", combined.Length);
    }
}
