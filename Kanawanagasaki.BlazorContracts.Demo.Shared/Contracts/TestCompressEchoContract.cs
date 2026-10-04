namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/compress/echo", EVerbs.Post)]
[ContractCompression]
public class TestCompressEchoContract : IContract<SimpleTestResponse>
{
    public string Message { get; init; } = string.Empty;
}
