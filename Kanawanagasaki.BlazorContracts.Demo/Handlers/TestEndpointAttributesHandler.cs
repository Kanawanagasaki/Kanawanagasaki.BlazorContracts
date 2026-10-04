namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;

[ExcludeFromDescription]
[OutputCache(NoStore = true)]
[IgnoreAntiforgeryToken]
public class TestEndpointAttributesHandler : IContractHandler<TestEndpointAttributesContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestEndpointAttributesContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<SimpleTestResponse>(new SimpleTestResponse { Message = "endpoint attributes ok" }));
}
