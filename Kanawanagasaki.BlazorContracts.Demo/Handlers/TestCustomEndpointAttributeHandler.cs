namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

[DemoAudit("demo.audit", Level = 3)]
public class TestCustomEndpointAttributeHandler : IContractHandler<TestCustomEndpointAttributeContract>
{
    public Task<ContractResult> HandleAsync(TestCustomEndpointAttributeContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(StatusCodes.Status200OK));
    }
}
