namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestGetCompressedStreamHandler : IContractHandler<TestGetCompressedStreamContract, Stream>
{
    public Task<ContractResult<Stream>> HandleAsync(TestGetCompressedStreamContract contract, CancellationToken ct = default)
    {
        var data = new byte[256];
        for (int i = 0; i < 256; i++)
            data[i] = (byte)i;

        return Task.FromResult(new ContractResult<Stream>(new MemoryStream(data)));
    }
}
