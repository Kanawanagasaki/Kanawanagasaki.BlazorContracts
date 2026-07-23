namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/post", EVerbs.Post)]
public class TestPostNoBodyNoResponseContract : IContract { }
