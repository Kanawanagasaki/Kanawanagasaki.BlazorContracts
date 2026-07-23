namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetSimpleResponseHandler : IContractHandler<TestGetSimpleResponseContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(TestGetSimpleResponseContract contract, CancellationToken ct = default)
    {
        var minute = DateTimeOffset.Now.Minute.ToString();

        var response = new SimpleTestResponse
        {
            Message = minute
        };

        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}
