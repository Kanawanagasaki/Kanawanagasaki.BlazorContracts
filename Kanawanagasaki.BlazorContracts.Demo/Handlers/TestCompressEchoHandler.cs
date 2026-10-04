namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestCompressEchoHandler : IContractHandler<TestCompressEchoContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestCompressEchoContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<SimpleTestResponse>(new SimpleTestResponse { Message = $"Compressed echo: {contract.Message}" }));
}
