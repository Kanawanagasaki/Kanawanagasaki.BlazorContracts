namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetComplexResponseHandler : IContractHandler<TestGetComplexResponseContract, ComplexResponse>
{
    public Task<ContractResult<ComplexResponse>> HandleAsync(TestGetComplexResponseContract contract, CancellationToken ct = default)
    {
        var minute = DateTimeOffset.UtcNow.Minute;
        var response = ComplexResponse.Create(minute);

        return Task.FromResult(new ContractResult<ComplexResponse>(response));
    }
}
