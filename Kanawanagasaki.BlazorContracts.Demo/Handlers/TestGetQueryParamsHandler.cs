namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetQueryParamsHandler : IContractHandler<TestGetQueryParamsContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetQueryParamsContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Filter={contract.Filter};Page={contract.Page};Enabled={contract.Enabled}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
