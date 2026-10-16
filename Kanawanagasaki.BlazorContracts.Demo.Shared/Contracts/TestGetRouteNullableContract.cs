namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/route-nullable/{Value}", EVerbs.Get)]
public class TestGetRouteNullableContract : IContract<SimpleTestResponse>
{
    public int? Value { get; init; }
}
