namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoClearAllHandler(AppStore store) : IContractHandler<TodoClearAllContract>
{
    private readonly AppStore _store = store;

    public Task<ContractResult> HandleAsync(TodoClearAllContract contract, CancellationToken ct = default)
    {
        _store.ClearTodos();
        return Task.FromResult(new ContractResult(StatusCodes.Status204NoContent));
    }
}
