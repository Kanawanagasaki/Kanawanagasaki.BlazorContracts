namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoGetAllHandler(AppStore store) : IContractHandler<TodoGetAllContract, TodoItem[]>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<TodoItem[]>> HandleAsync(TodoGetAllContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult<TodoItem[]>(_store.GetAllTodos()));
    }
}
