namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestGetCompressedBytesHandler : IContractHandler<TestGetCompressedBytesContract, byte[]>
{
    public Task<ContractResult<byte[]>> HandleAsync(TestGetCompressedBytesContract contract, CancellationToken ct = default)
    {
        var data = new byte[512];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)(i % 256);

        return Task.FromResult(new ContractResult<byte[]>(data));
    }
}
