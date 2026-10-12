namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestDeleteQueryHandler : IContractHandler<TestDeleteQueryContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestDeleteQueryContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Filter={contract.Filter};Page={contract.Page}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
