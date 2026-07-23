namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestGetNoDataHandler : IContractHandler<TestGetNoDataContract>
{
    public Task<ContractResult> HandleAsync(TestGetNoDataContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(204));
    }
}
