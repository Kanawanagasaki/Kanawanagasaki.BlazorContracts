namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/auth/login", EVerbs.Post)]
public class AuthLoginContract : IContract<AuthUserInfo>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
