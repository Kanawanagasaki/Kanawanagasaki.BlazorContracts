namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/{Id}", EVerbs.Get)]
public class TestGetRouteParamNoResponseContract : IContract
{
    public int Id { get; set; }
}
