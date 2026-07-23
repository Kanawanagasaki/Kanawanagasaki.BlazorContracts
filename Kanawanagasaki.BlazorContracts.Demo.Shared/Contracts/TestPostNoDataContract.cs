namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/simple-response-no-req", EVerbs.Post)]
public class TestPostNoDataContract : IContract<SimpleTestResponse> { }
