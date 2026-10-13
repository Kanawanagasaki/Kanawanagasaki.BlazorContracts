namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query-optional", EVerbs.Get)]
public class TestGetQueryOptionalContract : IContract<SimpleTestResponse>
{
    public string Text { get; init; } = string.Empty;
    public int? Count { get; init; }
    public bool Flag { get; init; }
}
