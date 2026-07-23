namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestGetRouteParamNoResponseHandler : IContractHandler<TestGetRouteParamNoResponseContract>
{
    public Task<ContractResult> HandleAsync(TestGetRouteParamNoResponseContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(contract.Id * 10));
    }
}
