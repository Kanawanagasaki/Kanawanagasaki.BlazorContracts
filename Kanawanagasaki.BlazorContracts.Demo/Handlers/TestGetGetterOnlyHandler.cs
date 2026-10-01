namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetGetterOnlyHandler : IContractHandler<TestGetGetterOnlyContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetGetterOnlyContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Echo={contract.Echo};Repeat={contract.Repeat}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
