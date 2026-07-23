namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/post-complex", EVerbs.Post)]
public class TestPostComplexBodyContract : IContract<ComplexResponse>
{
    public long Number { get; set; }
    public int Seed { get; set; }
    public string Name { get; set; } = string.Empty;
    public NumberDto[] Numbers { get; set; } = [];
}
