namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

public class TestMsgpackBytesEchoHandler : IContractHandler<TestMsgpackBytesEchoContract, byte[]>
{
    public Task<ContractResult<byte[]>> HandleAsync(TestMsgpackBytesEchoContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<byte[]>(contract.Payload));
}
