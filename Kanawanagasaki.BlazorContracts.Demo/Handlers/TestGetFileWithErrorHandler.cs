namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;

public class TestGetFileWithErrorHandler : IContractHandler<TestGetFileWithErrorContract, ContractFile>
{
    public Task<ContractResult<ContractFile>> HandleAsync(TestGetFileWithErrorContract contract, CancellationToken ct = default)
    {
        var payload = new byte[2400];
        for (var i = 0; i < payload.Length; i++)
            payload[i] = (byte)(i % 256);

        var stream = new MemoryStream(payload, writable: false);
        return Task.FromResult(new ContractResult<ContractFile>(StatusCodes.Status200OK, "Generated with a warning: source document contained unrenderable glyphs.")
        {
            Data = new ContractFile(stream, "réport-2026.pdf", "application/pdf", payload.Length)
        });
    }
}
