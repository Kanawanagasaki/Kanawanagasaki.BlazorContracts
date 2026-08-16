namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
public class AuthSecuredPutHandler : IContractHandler<AuthSecuredPutContract, string>
{
    public Task<ContractResult<string>> HandleAsync(AuthSecuredPutContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<string>($"Echo (PUT id={contract.Id}): {contract.Message}"));
}
