namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query-required-delete", EVerbs.Delete)]
public class TestDeleteQueryRequiredContract : IContract<SimpleTestResponse>
{
    public required string Id { get; init; }
    public string? Reason { get; init; }
}
