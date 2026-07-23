namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/update", EVerbs.Put)]
public class TestPutSimpleBodyContract : IContract<ComplexResponse>
{
    public long Number { get; set; }
    public int Seed { get; set; }
}
