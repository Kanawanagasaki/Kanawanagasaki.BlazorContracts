namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestPostNoBodyNoResponseHandler : IContractHandler<TestPostNoBodyNoResponseContract>
{
    public Task<ContractResult> HandleAsync(TestPostNoBodyNoResponseContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(204));
    }
}
