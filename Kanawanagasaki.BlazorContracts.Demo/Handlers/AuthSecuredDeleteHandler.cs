namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class AuthSecuredDeleteHandler : IContractHandler<AuthSecuredDeleteContract>
{
    public Task<ContractResult> HandleAsync(AuthSecuredDeleteContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult(200));
}
