namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class AuthAdminOnlyHandler : IContractHandler<AuthAdminOnlyContract, AuthUserInfo>
{
    public required IHttpContextAccessor HttpContextAccessor { get; init; }

    public Task<ContractResult<AuthUserInfo>> HandleAsync(AuthAdminOnlyContract contract, CancellationToken ct = default)
    {
        var user = HttpContextAccessor.HttpContext!.User;
        var result = new AuthUserInfo
        {
            UserName = user.Identity?.Name ?? "Unknown",
            Roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
            Claims = user.Claims.Select(c => $"{c.Type}: {c.Value}").ToArray(),
        };
        return Task.FromResult(new ContractResult<AuthUserInfo>(result));
    }
}
