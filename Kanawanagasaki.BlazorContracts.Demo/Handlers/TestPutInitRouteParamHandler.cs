namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPutInitRouteParamHandler : IContractHandler<TestPutInitRouteParamContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestPutInitRouteParamContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Id={contract.Id};Message={contract.Message}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
