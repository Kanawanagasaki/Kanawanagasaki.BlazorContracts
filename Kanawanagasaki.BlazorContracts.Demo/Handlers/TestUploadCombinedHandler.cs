namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestUploadCombinedHandler : IContractHandler<TestUploadCombinedContract, FileUploadResult[]>
{
    public Task<ContractResult<FileUploadResult[]>> HandleAsync(TestUploadCombinedContract contract, CancellationToken ct = default)
    {
        var list = new List<FileUploadResult>();

        if (contract.Data is not null && contract.Data.Length > 0)
        {
            var sha = System.Security.Cryptography.SHA256.HashData(contract.Data);
            list.Add(new FileUploadResult
            {
                FileName = "bytearray.data",
                Length = contract.Data.Length,
                MediaType = "application/octet-stream",
                Sha256 = Convert.ToHexString(sha)
            });
        }

        if (contract.File1 is not null)
        {
            using var ms = new MemoryStream();
            contract.File1.Stream.CopyTo(ms);
            var sha = System.Security.Cryptography.SHA256.HashData(ms.ToArray());
            list.Add(new FileUploadResult
            {
                FileName = contract.File1.FileName,
                Length = ms.Length,
                MediaType = contract.File1.MediaType,
                Sha256 = Convert.ToHexString(sha)
            });
        }

        if (contract.File2 is not null)
        {
            using var ms = new MemoryStream();
            contract.File2.Stream.CopyTo(ms);
            var sha = System.Security.Cryptography.SHA256.HashData(ms.ToArray());
            list.Add(new FileUploadResult
            {
                FileName = contract.File2.FileName,
                Length = ms.Length,
                MediaType = contract.File2.MediaType,
                Sha256 = Convert.ToHexString(sha)
            });
        }

        if (list.Count == 0)
            return Task.FromResult(new ContractResult<FileUploadResult[]>(StatusCodes.Status400BadRequest, "At least one data source is required."));

        return Task.FromResult(new ContractResult<FileUploadResult[]>(list.ToArray()));
    }
}
