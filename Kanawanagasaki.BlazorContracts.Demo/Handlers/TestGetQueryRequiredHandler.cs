namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetQueryRequiredHandler : IContractHandler<TestGetQueryRequiredContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetQueryRequiredContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Name={contract.Name};Note={contract.Note};Extra={contract.Extra}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
