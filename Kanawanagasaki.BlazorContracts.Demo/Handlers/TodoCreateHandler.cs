namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoCreateHandler(AppStore store) : IContractHandler<TodoCreateContract, TodoItem>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<TodoItem>> HandleAsync(TodoCreateContract contract, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contract.Title))
            return Task.FromResult(new ContractResult<TodoItem>(StatusCodes.Status400BadRequest, "Title is required."));

        var todo = _store.AddTodo(contract.Title.Trim(), contract.IsDone);
        return Task.FromResult(new ContractResult<TodoItem>(todo));
    }
}
