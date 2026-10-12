namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Text.Json.Serialization;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/delete-query", EVerbs.Delete)]
public class TestDeleteQueryContract : IContract<SimpleTestResponse>
{
    [JsonPropertyName("query_filter")]
    public string? Filter { get; init; }

    [JsonPropertyName("page_number")]
    public int Page { get; init; }
}
