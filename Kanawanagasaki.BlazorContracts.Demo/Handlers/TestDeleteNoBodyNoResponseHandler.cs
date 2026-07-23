namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestDeleteNoBodyNoResponseHandler : IContractHandler<TestDeleteNoBodyNoResponseContract>
{
    public Task<ContractResult> HandleAsync(TestDeleteNoBodyNoResponseContract contract, CancellationToken ct = default)
    {
        var workId = Random.Shared.Next(100000);
        return Task.FromResult(new ContractResult(204));
    }
}
