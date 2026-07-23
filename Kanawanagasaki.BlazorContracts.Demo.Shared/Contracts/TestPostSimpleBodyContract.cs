namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/post-simple", EVerbs.Post)]
public class TestPostSimpleBodyContract : IContract<SimpleTestResponse>
{
    public long Number { get; set; }
    public int Seed { get; set; }
}
