namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test", EVerbs.Get)]
public class TestGetNoDataContract : IContract { }
