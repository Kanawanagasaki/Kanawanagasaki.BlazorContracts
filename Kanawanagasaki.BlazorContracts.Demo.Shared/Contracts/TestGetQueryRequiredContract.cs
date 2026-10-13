namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Text.Json.Serialization;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query-required", EVerbs.Get)]
public class TestGetQueryRequiredContract : IContract<SimpleTestResponse>
{
    public required string Name { get; init; }
    [JsonPropertyName("note_text")]
    public required string? Note { get; init; }
    public string? Extra { get; init; }
}
