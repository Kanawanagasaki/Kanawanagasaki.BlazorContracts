namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoGetByIdHandler(AppStore store) : IContractHandler<TodoGetByIdContract, TodoItem>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<TodoItem>> HandleAsync(TodoGetByIdContract contract, CancellationToken ct = default)
    {
        var todo = _store.GetTodoById(contract.Id);
        if (todo is null)
            return Task.FromResult(new ContractResult<TodoItem>(StatusCodes.Status404NotFound, $"Todo with id={contract.Id} not found."));
        return Task.FromResult(new ContractResult<TodoItem>(todo));
    }
}
