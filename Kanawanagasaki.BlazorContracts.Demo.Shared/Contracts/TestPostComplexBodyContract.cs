namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/post-complex", EVerbs.Post)]
public class TestPostComplexBodyContract : IContract<ComplexResponse>
{
    public long Number { get; init; }
    public int Seed { get; init; }
    public string Name { get; init; } = string.Empty;
    public NumberDto[] Numbers { get; init; } = [];
}
