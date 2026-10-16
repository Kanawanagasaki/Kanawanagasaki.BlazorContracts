namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public enum TestRouteLevel
{
    Low,
    Medium,
    High
}

[Contract("/api/test/enum/{Level}", EVerbs.Get)]
public class TestGetRouteEnumContract : IContract<SimpleTestResponse>
{
    public TestRouteLevel Level { get; init; }
}
