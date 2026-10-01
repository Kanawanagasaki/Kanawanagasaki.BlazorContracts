namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query/{Id}", EVerbs.Get)]
public class TestGetRouteQueryParamContract : IContract<SimpleTestResponse>
{
    public int Id { get; init; }
    public string? Filter { get; init; }
    public int? Page { get; init; }
}
