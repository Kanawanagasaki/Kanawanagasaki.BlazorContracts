namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/disposable-response", EVerbs.Get)]
public class TestGetDisposableResponseContract : IContract<CustomDisposableResource> { }
