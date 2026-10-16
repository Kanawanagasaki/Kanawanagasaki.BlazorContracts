namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestRouteCollisionHandler : IContractHandler<TestRouteCollisionContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestRouteCollisionContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"count={contract.Count};q={contract.parsedValueCount}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
