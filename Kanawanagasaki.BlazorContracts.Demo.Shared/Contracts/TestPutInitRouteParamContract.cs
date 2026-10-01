namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/put-init-route/{Id}", EVerbs.Put)]
public class TestPutInitRouteParamContract : IContract<SimpleTestResponse>
{
    public int Id { get; init; }

    public string Message { get; init; } = string.Empty;
}
