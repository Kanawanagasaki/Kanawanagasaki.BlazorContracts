namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetMsgpackEnvelopeHandler : IContractHandler<TestGetMsgpackEnvelopeContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetMsgpackEnvelopeContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Msgpack envelope echo: {contract.Echo}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
