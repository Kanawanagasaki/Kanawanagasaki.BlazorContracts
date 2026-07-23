namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/update/{Seed}", EVerbs.Put)]
public class TestPutRouteParamContract : IContract<SimpleTestResponse>
{
    public long Number { get; set; }
    public int Seed { get; set; }
}
