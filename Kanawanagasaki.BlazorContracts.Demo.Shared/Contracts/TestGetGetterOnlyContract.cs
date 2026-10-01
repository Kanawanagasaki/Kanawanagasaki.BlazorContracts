namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/get-getter-only", EVerbs.Get)]
public class TestGetGetterOnlyContract : IContract<SimpleTestResponse>
{
    public string Echo { get; } = string.Empty;

    public int Repeat { get; private set; }
}
