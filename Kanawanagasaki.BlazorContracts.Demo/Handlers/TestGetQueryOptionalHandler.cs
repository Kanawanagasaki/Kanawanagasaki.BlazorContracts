namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetQueryOptionalHandler : IContractHandler<TestGetQueryOptionalContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetQueryOptionalContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Text={contract.Text};Count={contract.Count?.ToString() ?? "null"};Flag={contract.Flag}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
