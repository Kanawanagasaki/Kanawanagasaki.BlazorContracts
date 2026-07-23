namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPutRouteParamHandler : IContractHandler<TestPutRouteParamContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestPutRouteParamContract contract, CancellationToken ct = default)
    {
        var result = (contract.Number * contract.Seed).ToString();

        var response = new SimpleTestResponse
        {
            Message = result
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
