namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPutSimpleBodyHandler : IContractHandler<TestPutSimpleBodyContract, ComplexResponse>
{
    public Task<ContractResult<ComplexResponse>> HandleAsync(TestPutSimpleBodyContract contract, CancellationToken ct = default)
    {
        var seed = (int)(contract.Number ^ contract.Seed);
        var response = ComplexResponse.Create(seed);

        return Task.FromResult(new ContractResult<ComplexResponse>(response));
    }
}
