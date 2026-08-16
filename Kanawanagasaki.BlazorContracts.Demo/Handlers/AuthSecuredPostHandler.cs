namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class AuthSecuredPostHandler : IContractHandler<AuthSecuredPostContract, string>
{
    public Task<ContractResult<string>> HandleAsync(AuthSecuredPostContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<string>($"Echo (POST): {contract.Message}"));
}
