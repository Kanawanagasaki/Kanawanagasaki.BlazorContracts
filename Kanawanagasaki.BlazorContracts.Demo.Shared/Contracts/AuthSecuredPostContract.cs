namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/auth/secured-post", EVerbs.Post)]
public class AuthSecuredPostContract : IContract<string>
{
    public string Message { get; init; } = string.Empty;
}
