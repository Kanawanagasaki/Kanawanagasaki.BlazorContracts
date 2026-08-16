namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/auth/secured-delete/{Id}", EVerbs.Delete)]
public class AuthSecuredDeleteContract : IContract
{
    public int Id { get; set; }
}
