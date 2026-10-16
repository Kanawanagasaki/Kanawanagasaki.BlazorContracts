namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetRouteNullableHandler : IContractHandler<TestGetRouteNullableContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetRouteNullableContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = contract.Value is null
                ? "Value=null"
                : $"Value={contract.Value}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
