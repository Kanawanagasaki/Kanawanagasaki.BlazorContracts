namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/auth/secured-put", EVerbs.Put)]
public class AuthSecuredPutContract : IContract<string>
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
}
