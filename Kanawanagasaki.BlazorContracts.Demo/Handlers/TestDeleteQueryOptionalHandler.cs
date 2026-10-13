namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestDeleteQueryOptionalHandler : IContractHandler<TestDeleteQueryOptionalContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestDeleteQueryOptionalContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Tag={contract.Tag};Limit={contract.Limit?.ToString() ?? "null"}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
