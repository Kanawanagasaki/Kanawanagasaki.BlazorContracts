namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/route-query-collision/{Count}", EVerbs.Get)]
public class TestRouteCollisionContract : IContract<SimpleTestResponse>
{
    public int Count { get; init; }
    public int parsedValueCount { get; init; }
}
