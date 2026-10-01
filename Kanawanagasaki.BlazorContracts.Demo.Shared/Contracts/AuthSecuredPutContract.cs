namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/auth/secured-put", EVerbs.Put)]
public class AuthSecuredPutContract : IContract<string>
{
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
}
