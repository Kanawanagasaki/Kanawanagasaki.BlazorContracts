namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/stream", EVerbs.Get)]
public class TestGetStreamResponseContract : IContract<Stream> { }
