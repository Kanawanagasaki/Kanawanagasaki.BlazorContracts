namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestCompressBinaryHandler : IContractHandler<TestCompressBinaryContract, byte[]>
{
    public Task<ContractResult<byte[]>> HandleAsync(TestCompressBinaryContract contract, CancellationToken ct = default)
    {
        var prefix = "echo:"u8.ToArray();
        var data = new byte[prefix.Length + contract.Content.Length];
        prefix.CopyTo(data, 0);
        contract.Content.CopyTo(data, prefix.Length);

        return Task.FromResult(new ContractResult<byte[]>(data));
    }
}
