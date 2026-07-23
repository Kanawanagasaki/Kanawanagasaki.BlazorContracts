namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPostNoDataHandler : IContractHandler<TestPostNoDataContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestPostNoDataContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = "Server processed POST with no data"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
