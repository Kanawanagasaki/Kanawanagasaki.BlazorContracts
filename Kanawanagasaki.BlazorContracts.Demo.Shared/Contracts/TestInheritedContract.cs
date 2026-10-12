namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public abstract class TestInheritedBase
{
    public string Title { get; init; } = string.Empty;

    public int Count { get; init; }
}

[Contract("/api/test/post-inherited", EVerbs.Post)]
public class TestPostInheritedContract : TestInheritedBase, IContract<SimpleTestResponse>
{
    public string Suffix { get; init; } = string.Empty;
}
