namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestUploadMultipleFilesHandler : IContractHandler<TestUploadMultipleFilesContract, FileUploadResult[]>
{
    public Task<ContractResult<FileUploadResult[]>> HandleAsync(TestUploadMultipleFilesContract contract, CancellationToken ct = default)
    {
        var results = new FileUploadResult[0];

        if (contract.File1 is null && contract.File2 is null)
            return Task.FromResult(new ContractResult<FileUploadResult[]>(StatusCodes.Status400BadRequest, "At least one file is required."));

        var list = new List<FileUploadResult>();

        if (contract.File1 is not null)
        {
            using var ms1 = new MemoryStream();
            contract.File1.Stream.CopyTo(ms1);
            var data1 = ms1.ToArray();
            var sha1 = System.Security.Cryptography.SHA256.HashData(data1);
            list.Add(new FileUploadResult
            {
                FileName = contract.File1.FileName,
                Length = data1.Length,
                MediaType = contract.File1.MediaType,
                Sha256 = Convert.ToHexString(sha1)
            });
        }

        if (contract.File2 is not null)
        {
            using var ms2 = new MemoryStream();
            contract.File2.Stream.CopyTo(ms2);
            var data2 = ms2.ToArray();
            var sha2 = System.Security.Cryptography.SHA256.HashData(data2);
            list.Add(new FileUploadResult
            {
                FileName = contract.File2.FileName,
                Length = data2.Length,
                MediaType = contract.File2.MediaType,
                Sha256 = Convert.ToHexString(sha2)
            });
        }

        return Task.FromResult(new ContractResult<FileUploadResult[]>(list.ToArray()));
    }
}
