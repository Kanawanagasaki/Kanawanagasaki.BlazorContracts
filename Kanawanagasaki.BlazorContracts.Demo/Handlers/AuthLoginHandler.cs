namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class AuthLoginHandler : IContractHandler<AuthLoginContract, AuthUserInfo>
{
    public required IHttpContextAccessor HttpContextAccessor { get; init; }

    public async Task<ContractResult<AuthUserInfo>> HandleAsync(AuthLoginContract contract, CancellationToken ct = default)
    {
        List<Claim> claims;
        string[] roles;

        if (contract.Username == "admin" && contract.Password == "adminpass")
        {
            claims =
            [
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
            ];
            roles = ["Admin"];
        }
        else if (contract.Username == "user" && contract.Password == "userpass")
        {
            claims =
            [
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User"),
            ];
            roles = ["User"];
        }
        else
        {
            return new ContractResult<AuthUserInfo>(401, "Invalid username or password.");
        }

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContextAccessor.HttpContext!.SignInAsync("Cookies", principal);

        return new ContractResult<AuthUserInfo>(new AuthUserInfo
        {
            UserName = contract.Username,
            Roles = roles,
            Claims = claims.Select(c => $"{c.Type}: {c.Value}").ToArray(),
        });
    }
}
