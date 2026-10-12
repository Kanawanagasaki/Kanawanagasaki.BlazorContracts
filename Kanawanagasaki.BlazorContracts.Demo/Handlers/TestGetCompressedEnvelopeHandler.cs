namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetCompressedEnvelopeHandler : IContractHandler<TestGetCompressedEnvelopeContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetCompressedEnvelopeContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Compressed envelope echo: {contract.Echo}"
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
