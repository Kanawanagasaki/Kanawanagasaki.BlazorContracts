namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/remove", EVerbs.Delete)]
public class TestDeleteNoBodyNoResponseContract : IContract { }
