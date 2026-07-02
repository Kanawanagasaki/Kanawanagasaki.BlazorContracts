namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;

public class HealthCheckHandler : IContractHandler<HealthCheckContract>
{
    public Task<ContractResult> HandleAsync(HealthCheckContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(StatusCodes.Status200OK));
    }
}
