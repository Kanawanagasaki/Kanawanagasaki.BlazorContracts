namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestDeleteQueryRequiredHandler : IContractHandler<TestDeleteQueryRequiredContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestDeleteQueryRequiredContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Id={contract.Id};Reason={contract.Reason}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
