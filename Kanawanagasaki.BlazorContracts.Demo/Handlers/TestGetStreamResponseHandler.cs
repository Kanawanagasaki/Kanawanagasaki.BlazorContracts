namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestGetStreamResponseHandler : IContractHandler<TestGetStreamResponseContract, Stream>
{
    public Task<ContractResult<Stream>> HandleAsync(TestGetStreamResponseContract contract, CancellationToken ct = default)
    {
        var data = new byte[256];
        for (int i = 0; i < 256; i++)
            data[i] = (byte)i;

        var stream = new MemoryStream(data);
        return Task.FromResult(new ContractResult<Stream>(stream));
    }
}
