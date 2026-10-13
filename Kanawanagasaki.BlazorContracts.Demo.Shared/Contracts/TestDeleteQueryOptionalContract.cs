namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query-optional-delete", EVerbs.Delete)]
public class TestDeleteQueryOptionalContract : IContract<SimpleTestResponse>
{
    public string Tag { get; init; } = string.Empty;
    public int? Limit { get; init; }
}
