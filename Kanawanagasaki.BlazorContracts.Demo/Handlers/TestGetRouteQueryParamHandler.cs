namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetRouteQueryParamHandler : IContractHandler<TestGetRouteQueryParamContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetRouteQueryParamContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"{contract.Id}{contract.Filter}{contract.Page}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
