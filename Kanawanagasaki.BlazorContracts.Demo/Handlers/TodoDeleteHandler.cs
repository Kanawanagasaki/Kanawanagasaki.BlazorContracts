namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoDeleteHandler(AppStore store) : IContractHandler<TodoDeleteContract>
{
    private readonly AppStore _store = store;

    public Task<ContractResult> HandleAsync(TodoDeleteContract contract, CancellationToken ct = default)
    {
        var deleted = _store.DeleteTodo(contract.Id);
        if (!deleted)
            return Task.FromResult(new ContractResult(StatusCodes.Status404NotFound, $"Todo with id={contract.Id} not found."));
        return Task.FromResult(new ContractResult(StatusCodes.Status204NoContent));
    }
}
