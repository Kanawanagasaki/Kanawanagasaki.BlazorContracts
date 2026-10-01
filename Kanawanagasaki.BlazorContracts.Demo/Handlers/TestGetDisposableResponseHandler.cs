namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Text;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetDisposableResponseHandler : IContractHandler<TestGetDisposableResponseContract, CustomDisposableResource>
{
    public Task<ContractResult<CustomDisposableResource>> HandleAsync(TestGetDisposableResponseContract contract, CancellationToken ct = default)
    {
        var resource = new CustomDisposableResource
        {
            Content = "Disposable resource payload"u8.ToArray()
        };

        return Task.FromResult<ContractResult<CustomDisposableResource>>(new ContractResult<CustomDisposableResource>(resource));
    }
}
