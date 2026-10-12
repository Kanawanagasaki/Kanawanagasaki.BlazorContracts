namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/route-string/{Name}", EVerbs.Get)]
public class TestGetRouteStringContract : IContract<SimpleTestResponse>
{
    public string Name { get; init; } = string.Empty;
}
