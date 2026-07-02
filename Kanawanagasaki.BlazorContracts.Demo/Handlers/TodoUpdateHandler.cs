namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoUpdateHandler(AppStore store) : IContractHandler<TodoUpdateContract, TodoItem>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<TodoItem>> HandleAsync(TodoUpdateContract contract, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contract.Title))
            return Task.FromResult(new ContractResult<TodoItem>(StatusCodes.Status400BadRequest, "Title is required."));

        var todo = _store.UpdateTodo(contract.Id, contract.Title.Trim(), contract.IsDone);
        if (todo is null)
            return Task.FromResult(new ContractResult<TodoItem>(StatusCodes.Status404NotFound, $"Todo with id={contract.Id} not found."));

        return Task.FromResult(new ContractResult<TodoItem>(todo));
    }
}
