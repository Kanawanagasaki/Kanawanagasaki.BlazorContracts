namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestUploadSingleFileHandler : IContractHandler<TestUploadSingleFileContract, FileUploadResult>
{
    public Task<ContractResult<FileUploadResult>> HandleAsync(TestUploadSingleFileContract contract, CancellationToken ct = default)
    {
        if (contract.File is null || contract.File.Stream.Length == 0)
            return Task.FromResult(new ContractResult<FileUploadResult>(StatusCodes.Status400BadRequest, "File is required."));

        byte[] content;
        using var ms = new MemoryStream();
        contract.File.Stream.CopyTo(ms);
        content = ms.ToArray();

        var sha256 = System.Security.Cryptography.SHA256.HashData(content);

        var result = new FileUploadResult
        {
            FileName = contract.File.FileName,
            Length = content.Length,
            MediaType = contract.File.MediaType,
            Sha256 = Convert.ToHexString(sha256)
        };

        return Task.FromResult(new ContractResult<FileUploadResult>(result));
    }
}
