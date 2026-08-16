namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/auth/admin-only", EVerbs.Get)]
public class AuthAdminOnlyContract : IContract<AuthUserInfo> { }
