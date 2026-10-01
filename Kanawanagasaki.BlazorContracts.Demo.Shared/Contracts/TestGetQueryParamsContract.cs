namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Text.Json.Serialization;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/query-params", EVerbs.Get)]
public class TestGetQueryParamsContract : IContract<SimpleTestResponse>
{
    [JsonPropertyName("query_filter")]
    public string? Filter { get; init; }

    [JsonPropertyName("page_number")]
    public int Page { get; init; }

    public bool Enabled { get; init; } = true;
}
