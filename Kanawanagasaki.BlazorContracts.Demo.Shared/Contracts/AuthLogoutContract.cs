namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/auth/logout", EVerbs.Post)]
public class AuthLogoutContract : IContract { }
