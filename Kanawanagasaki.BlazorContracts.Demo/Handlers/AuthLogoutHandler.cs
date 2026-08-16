namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

public class AuthLogoutHandler : IContractHandler<AuthLogoutContract>
{
    public required IHttpContextAccessor HttpContextAccessor { get; init; }

    public async Task<ContractResult> HandleAsync(AuthLogoutContract contract, CancellationToken ct = default)
    {
        await HttpContextAccessor.HttpContext!.SignOutAsync("Cookies");
        return new ContractResult(200);
    }
}
