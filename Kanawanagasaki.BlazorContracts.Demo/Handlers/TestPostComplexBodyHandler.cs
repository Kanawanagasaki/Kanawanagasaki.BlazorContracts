namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPostComplexBodyHandler : IContractHandler<TestPostComplexBodyContract, ComplexResponse>
{
    public Task<ContractResult<ComplexResponse>> HandleAsync(TestPostComplexBodyContract contract, CancellationToken ct = default)
    {
        var response = ComplexResponse.Create(contract.Seed);

        return Task.FromResult(new ContractResult<ComplexResponse>(response));
    }
}
