namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/custom-endpoint-attribute", EVerbs.Get)]
public class TestCustomEndpointAttributeContract : IContract { }
