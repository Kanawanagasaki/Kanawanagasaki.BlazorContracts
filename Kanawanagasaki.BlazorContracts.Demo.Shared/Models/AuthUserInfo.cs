namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class AuthUserInfo
{
    public string UserName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
    public string[] Claims { get; set; } = [];
}
