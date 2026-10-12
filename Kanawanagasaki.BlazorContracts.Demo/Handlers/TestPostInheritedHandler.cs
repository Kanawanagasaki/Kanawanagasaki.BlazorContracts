namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestPostInheritedHandler : IContractHandler<TestPostInheritedContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestPostInheritedContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"{contract.Title}#{contract.Count}#{contract.Suffix}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
