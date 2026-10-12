namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetRouteStringHandler : IContractHandler<TestGetRouteStringContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetRouteStringContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = contract.Name
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
